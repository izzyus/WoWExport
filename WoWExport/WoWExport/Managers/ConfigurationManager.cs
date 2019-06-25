using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managers
{
    class ConfigurationManager
    {
        public static String OutputDirectory;

        //ADT Exporter
        public static Boolean ADTExportM2 = false;
        public static Boolean ADTExportWMO = false;
        public static Boolean ADTExportFoliage = false; //Obsolete atm
        public static Boolean ADTexportTextures = false;
        public static Boolean ADTexportAlphaMaps = false;

        //WMO Export
        public static Boolean WMOExportM2 = false;
    }
}
