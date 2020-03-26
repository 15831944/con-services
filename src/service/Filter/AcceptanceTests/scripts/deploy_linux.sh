cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache VSS.Visionlink.Filter.AcceptanceTests.sln

cd tests
dotnet publish WebApiTests/WebApiTests.csproj -o ../../deploy/WebApiTests -f netcoreapp3.1
dotnet publish ExecutorTests/ExecutorTests.csproj -o ../../deploy/ExecutorTests -f netcoreapp3.1
dotnet publish RepositoryTests/RepositoryTests.csproj -o ../../deploy/RepositoryTests -f netcoreapp3.1

