﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Gateway.WebApi.ActionServices;
using VSS.TRex.Tests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Gateway.Tests.ActionServices
{
  public class CoordinateServiceUtilityTests : IClassFixture<DILoggingFixture>, IDisposable
  {
    /* Settings for Alpha
    private const string TPAAS_AUTH_URL = "https://identity-stg.trimble.com/i/oauth2";
    private const string TPAAS_APP_TOKEN = "YjZvSk1WemZ6ajJNUEplQkIyWlVLOUJ4ckZFYTp5MWk1YmZlMjdpQlRQb0VCS2laYXMxM0hacUFh";
    */

    public CoordinateServiceUtilityTests()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddTransient<ICoordinateServiceUtility, CoordinateServiceUtility>())
        .Add(x => x.AddSingleton<ITPaasProxy, TPaasProxy>())

/*        .Add(x => x.AddHttpClient<ITPaaSClient, TPaaSClient>(client =>
          client.BaseAddress = new Uri(TPAAS_AUTH_URL) //Configuration.GetValueString(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY))
        ).ConfigurePrimaryHttpMessageHandler(() => new TPaaSApplicationCredentialsRequestHandler
          {
            TPaaSToken = TPAAS_APP_TOKEN, //Configuration.GetValueString(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN_ENV_KEY),
            InnerHandler = new HttpClientHandler()
          }))
        .Add(x => x.AddTransient(context => new TPaaSAuthenticatedRequestHandler {TPaaSClient = context.GetService<ITPaaSClient>()}))
        .Add(x => x.AddSingleton<IConvertCoordinates>(new ConvertCoordinates(new CoreX.Wrapper.CoreX())))
*/
        .Complete();
    }

    [Fact]
    public async Task MachineStatusConvertCoords_NoMachines()
    {
      var machines = new List<MachineStatus>();

      var result = await DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(TestCommonConsts.DIMENSIONS_2012_DC_CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
    }

    [Fact]
    public async Task MachineStatusConvertCoords_Machines_DefaultNEE()
    {
      var machines = new List<MachineStatus>()
      {
        new MachineStatus(Consts.NULL_LEGACY_ASSETID, "Machine Name",
          false, string.Empty, null, null,
          Consts.NullDouble, Consts.NullDouble,
          Consts.NullDouble, Consts.NullDouble, Guid.NewGuid())
      };

      var result = await DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(TestCommonConsts.DIMENSIONS_2012_DC_CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
    }


    [Fact(Skip = "Skip until coreX is available")]
    public async Task MachineStatusConvertCoords_OneMachine()
    {
      var machines = new List<MachineStatus>() {new MachineStatus
        (Consts.NULL_LEGACY_ASSETID, "", false, "", null, null,
          lastKnownLatitude: null, lastKnownLongitude: null,
          lastKnownX: 2313, lastKnownY: 1204)
      };

      var result = await DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(TestCommonConsts.DIMENSIONS_2012_DC_CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      machines[0].lastKnownLongitude.Should().Be(-115.03727717865179);
      machines[0].lastKnownLatitude.Should().Be(36.217306995697705);
    }

    [Fact(Skip = "Skip until coreX is available")]
    public async Task MachineStatusConvertCoords_TwoMachine()
    {
      var machines = new List<MachineStatus>() {new MachineStatus
        (Consts.NULL_LEGACY_ASSETID, "machine1", false, "", null, null,
          lastKnownLatitude: null, lastKnownLongitude: null,
          lastKnownX: 2313, lastKnownY: 1204),
        new MachineStatus
        (Consts.NULL_LEGACY_ASSETID, "machine2", false, "", null, null,
          lastKnownLatitude: null, lastKnownLongitude: null,
          lastKnownX: 2314, lastKnownY: 1205)
      };

      var result = await DIContext.Obtain<ICoordinateServiceUtility>().PatchLLH(TestCommonConsts.DIMENSIONS_2012_DC_CSIB, machines);
      result.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      machines[0].lastKnownLongitude.Should().Be(-115.05065884125976);
      machines[0].lastKnownLatitude.Should().Be(36.196461918456677);
      machines[1].lastKnownLongitude.Should().Be(-115.05065884125976);
      machines[1].lastKnownLatitude.Should().Be(36.196461918456677);
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
