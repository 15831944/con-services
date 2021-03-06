Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
choco install kubernetes-helm
choco install kubernetes-cli
helm init --client-only
helm repo add incubator http://storage.googleapis.com/kubernetes-charts-incubator
mkdir $HOME\Documents\.kube\
cp config $HOME\Documents\.kube\config
#Sometimes it tries to use this folder. Why?? #ADAM's NOTE: You need to set $env:KUBECONFIG
mkdir $HOME\.kube\
cp config $HOME\.kube\config