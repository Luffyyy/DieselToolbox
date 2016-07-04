using DieselEngineFormats.Bundle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DieselEngineFormats
{
    public static class Defs
    {
        public static BundleHeaderConfig PD2Config = new BundleHeaderConfig
        {
            HeaderStartLengthTag = 909195826,
            HeaderEndLengthTag = 1699008,
            EntryStartLengthTag = 1699073,
            EndTag = 2495946521,
            EmptyEndInt = true,
            EntryStartTag = 1698812,
            ReferenceStartTag = 3959211512
        };

        public static BundleHeaderConfig PD2ConfigPreU70 = new BundleHeaderConfig
        {

        };

        public static BundleHeaderConfig PDTHConfig = new BundleHeaderConfig
        {
            HeaderStartLengthTag = 229179392,
            HeaderEndLengthTag = 1633928,
            EntryStartLengthTag = 1,
            EntryStartTag = 1633936,
            ReferenceStartTag = 3959211512,
            EndTag = 2495946521,
        };

        public static List<BundleHeaderConfig> ConfigLookup = new List<BundleHeaderConfig> { PD2Config, PD2ConfigPreU70, PDTHConfig };
    }
}
