FROM microsoft/dotnet:2.1.300-rc1-sdk-alpine3.7 as build_container

#Copy files from scm into build container and build
COPY . /build/

####### TODO run tests


# Build 
RUN dotnet publish /build/TRex.netstandard.sln --output /trex

# #Now create runtime container
# Why bash? it seems to be needed for ignite to run (maybe a dependency?)
FROM microsoft/dotnet:2.1.0-rc1-runtime-alpine3.7
RUN \
  apk update && \
  apk upgrade && \
  apk add openjdk8 && \
  apk add bash && \
  rm -rf /var/cache/apk/*

# #Need these for ignite to work
ENV JAVA_HOME=/usr/lib/jvm/java-1.8-openjdk
ENV LD_LIBRARY_PATH=$JAVA_HOME/jre/lib/amd64/server

# # Copy built artifacts from last stage into runtime container
# # TRex still cannot run in 
COPY --from=build_container /trex/ /trex/
#Get the ignite libs as these are ignored by dotnot publish
COPY --from=build_container /root/.nuget/packages/apache.ignite/2.4.0/libs/ /trex/libs/



