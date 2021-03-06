#!/bin/bash
dotnet clean TRex.Framework.sln
dotnet restore TRex.Framework.sln
dotnet build TRex.Framework.sln

cd tools

dotnet minicover instrument --workdir ../ --assemblies tests/**/bin/**/*.dll --sources **/*.cs --exclude-assemblies **/*.Tests.*.dll

dotnet minicover reset

cd ..
                                          
dotnet test --no-build TRex.Framework.sln -p:ParallelizeTestCollections=false -r test_results
cd tools

dotnet minicover uninstrument --workdir ../

dotnet minicover htmlreport --workdir ../ --threshold 90

dotnet minicover report --workdir ../ --threshold 90

cd ..