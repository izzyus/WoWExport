using System;

namespace Managers
{
    class ConfigurationManager
    {
        public static int Profile;
        public static String GameDir;

        public static String OutputDirectory;

        //ADT Exporter
        public static Boolean ADTExportM2 = false;
        public static Boolean ADTExportWMO = false;
        public static Boolean ADTExportFoliage = false; //Obsolete atm
        public static Boolean ADTexportTextures = false;
        public static Boolean ADTexportAlphaMaps = false;
        public static Boolean ADTIgnoreHoles = false;
        public static int ADTAlphaMode = 0;
        public static Boolean ADTAlphaUseA = false;
        public static Boolean ADTModelsPlacementGlobalPath = false;
        public static String ADTQuality = "low";
        public static Boolean ADTSplitChunks = false;
        public static Boolean ADTPreserveTextureStruct = false;
        public static Boolean ADTExportSpecularTextures = false;

        //WMO Export
        public static Boolean WMOExportM2 = false;
        public static Boolean WMODoodadsGlobalPath = false;
        public static Boolean WMODoodadsPlacementGlobalPath = false;
    }
}
