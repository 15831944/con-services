cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore --no-cache ConsumerAcceptanceTests.sln

cd tests
dotnet publish EventTests/EventTests.csproj -o ..\..\deploy\EventTests -f netcoreapp3.1
dotnet publish KafkaTests/KafkaTests.csproj -o ..\..\deploy\KafkaTests -f netcoreapp3.1
dotnet publish RepositoryTests/RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp3.1
dotnet publish RepositoryLandfillTests/RepositoryLandfillTests.csproj -o ..\..\deploy\RepositoryLandfillTests -f netcoreapp3.1

copy KafkaTests\appsettings.json ..\deploy\KafkaTests\
copy RepositoryTests\appsettings.json ..\deploy\RepositoryTests\
copy RepositoryLandfillTests\appsettings.json ..\deploy\RepositoryLandfillTests\

cd ..
cd utilities
dotnet publish TestRun/TestRun.csproj -o ..\..\deploy\TestRun -f netcoreapp3.1
cd ..