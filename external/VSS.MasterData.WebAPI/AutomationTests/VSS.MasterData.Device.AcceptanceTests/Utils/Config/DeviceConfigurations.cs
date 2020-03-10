﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Device.AcceptanceTests.Utils.Config
{
    public enum DeviceType
    {
        MANUALDEVICE = 0,
        PL121 = 1,
        PL321 = 2,
        Series522 = 3,
        Series523 = 4,
        Series521 = 5,
        SNM940 = 6,
        CrossCheck = 7,
        TrimTrac = 8,
        PL420 = 9,
        PL421 = 10,
        TM3000 = 11,
        TAP66 = 12,
        SNM451 = 13,
        PL431 = 14,
        DCM300 = 15,
        PL641 = 16,
        PLE641 = 17,
        PLE641PLUSPL631 = 18,
        PLE631 = 19,
        PL631 = 20,
        PL241 = 21,
        PL231 = 22,
        BasicVirtualDevice = 23,
        MTHYPHEN10 = 24,
        XT5060 = 25,
        XT4860 = 26,
        TTUSeries = 27,
        XT2000 = 28,
        MTGModularGatewayHYPHENMotorEngine = 29,
        MTGModularGatewayHYPHENElectricEngine = 30,
        MCHYPHEN3 = 31,
        Dummy = 32,
        XT6540 = 33,
        XT65401 = 34,
        XT65402 = 35,
        THREEPDATA = 36,
        PL131 = 37,
        PL141 = 38,
        PL440 = 39,
        PLE601 = 40,
        PL161 = 41,
        PL240 = 42,
        PL542 = 43,
        PLE642 = 44,
        PLE742 = 45,
        SNM941 = 46,
        PL240B = 47,
        TAP76 = 48
    }

    public enum DeviceState
    {
        Installed = 1,
        Provisioned = 2,
        Subscribed = 3,
        DeregisteredTechnician = 4,
        DeregisteredStore = 5
    }
}