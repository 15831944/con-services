# Get the ip address from the network adaptor 
$ipV4 = ( Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway -ne $null -and  $_.NetAdapter.Status -ne "Disconnected"}).IPv4Address.IPAddress
$ipV4

(Get-Content docker-compose-local.env) | Foreach-Object {$_ -replace "LOCALIPADDRESS", $ipV4} | Set-Content docker-compose-local.env
(Get-Content build/yaml/testing-configmap.yaml) | Foreach-Object {$_ -replace "LOCALIPADDRESS", $ipV4} | Set-Content build/yaml/testing-configmap.yaml
