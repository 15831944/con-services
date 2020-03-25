cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache FileAccessAcceptanceTests.sln

cd tests
dotnet publish IntegrationTests/IntegrationTests.csproj -o ../../deploy/IntegrationTests -f netcoreapp3.1
cd ..