FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-sdk as builder
ARG SERVICE_PATH

COPY . /build/
WORKDIR /build

RUN dotnet publish ./$SERVICE_PATH/Service/VSS.Productivity3D.3DAssetMgmt/VSS.Productivity3D.AssetMgmt3D.csproj -o /build/artifacts/WebApi -f netcoreapp3.1

FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:dotnet-3.1-aspnetcore-runtime

#This is required for the webpi to run properly
RUN apt-get update && apt-get install -y \
    libunwind8 \
    && rm -rf /var/lib/apt/lists/*


ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so 

COPY ./build/newrelic /newrelic/

RUN dpkg -i /newrelic/newrelic-netcore20*.deb

RUN ls -la /usr/local/newrelic-netcore20-agent

WORKDIR /app

ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

COPY --from=builder /build/artifacts/WebApi .

ENTRYPOINT ["dotnet", "VSS.Productivity3D.AssetMgmt3D.dll"]
