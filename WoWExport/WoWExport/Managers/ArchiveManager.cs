﻿using System;
using System.Collections.Generic;
using System.IO;
using StormLibSharp;
using CASCLib;

namespace Managers
{
    class ArchiveManager
    {
        public static Boolean usingCasc = false;
        public static CASCHandler cascHandler;
        public static String listFilePath;

        public static List<String> MainListFile = new List<String>();

        public static string locale;

        //Mysts
        public static MpqArchive expansion4;
        public static MpqArchive model;

        //Cataclysm
        public static MpqArchive art;
        public static MpqArchive expansion1;
        public static MpqArchive expansion2;
        public static MpqArchive expansion3;
        public static MpqArchive world;
        public static MpqArchive world2;

        //Locales
        public static MpqArchive locale1;

        //Lich King
        public static MpqArchive common;
        public static MpqArchive common2;
        public static MpqArchive expansion;
        public static MpqArchive lichking;
        public static MpqArchive patch;
        public static MpqArchive patch2;
        public static MpqArchive patch3;

        //Vanilla
        public static MpqArchive baseMPQ;
        public static MpqArchive dbc;
        public static MpqArchive interfaceMPQ;
        public static MpqArchive misc;
        public static MpqArchive sound;
        public static MpqArchive terrain;
        public static MpqArchive texture;
        public static MpqArchive wmo;

        //----------------------------------------------------------------------------------
        //  CASC
        //----------------------------------------------------------------------------------
        public static void LoadCASC()
        {
            if (ConfigurationManager.GameDir == null)
            {
                throw new Exception("Game directory not initialized, unable to load CASC");
            }
            try
            {
                cascHandler = CASCHandler.OpenLocalStorage(ConfigurationManager.GameDir);

                //Download a listfile if none exists
                if (!File.Exists(listFilePath))
                {
                    WoWExport.Listfile.Update();
                }

                cascHandler.Root.LoadListFile(listFilePath);
                //Console.WriteLine(cascHandler.Config.BuildName);

                CASCConfig.ValidateData = false;
                CASCConfig.ThrowOnFileNotFound = false;
                //--------------------------------------------------------------------------
                //  TO DO: FIND A WAY TO GET THE LOCALE
                //--------------------------------------------------------------------------
                cascHandler.Root.SetFlags(LocaleFlags.enUS); //Hardcoded for the moment
                //--------------------------------------------------------------------------
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //----------------------------------------------------------------------------------
        //  MPQ
        //----------------------------------------------------------------------------------
        #region MPQLoading-Assigning
        public static void LoadArchives()
        {
            if (ConfigurationManager.GameDir == null)
            {
                throw new Exception("Game directory not initialized, unable to load archives");
            }
            else
            {
                //Mysts
                expansion4 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\expansion4.MPQ", FileAccess.Read);
                model = new MpqArchive(ConfigurationManager.GameDir + @"\Data\model.MPQ", FileAccess.Read);

                //Cataclysm
                art = new MpqArchive(ConfigurationManager.GameDir + @"\Data\art.mpq", FileAccess.Read);
                expansion1 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\expansion1.MPQ", FileAccess.Read);
                expansion2 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\expansion2.MPQ", FileAccess.Read);
                expansion3 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\expansion3.MPQ", FileAccess.Read);
                world = new MpqArchive(ConfigurationManager.GameDir + @"\Data\world.mpq", FileAccess.Read);
                world2 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\world2.mpq", FileAccess.Read);

                locale1 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\" + locale + @"\locale-" + locale + ".MPQ", FileAccess.Read);

                //Lich King
                common = new MpqArchive(ConfigurationManager.GameDir + @"\Data\common.MPQ", FileAccess.Read);
                common2 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\common-2.MPQ", FileAccess.Read);
                expansion = new MpqArchive(ConfigurationManager.GameDir + @"\Data\expansion.MPQ", FileAccess.Read);
                lichking = new MpqArchive(ConfigurationManager.GameDir + @"\Data\lichking.MPQ", FileAccess.Read);
                patch = new MpqArchive(ConfigurationManager.GameDir + @"\Data\patch.MPQ", FileAccess.Read);
                patch2 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\patch-2.MPQ", FileAccess.Read);
                patch3 = new MpqArchive(ConfigurationManager.GameDir + @"\Data\patch-3.MPQ", FileAccess.Read);

                //Vanilla....
                baseMPQ = new MpqArchive(ConfigurationManager.GameDir + @"\Data\base.mpq", FileAccess.Read);
                dbc = new MpqArchive(ConfigurationManager.GameDir + @"\Data\dbc.mpq", FileAccess.Read);
                interfaceMPQ = new MpqArchive(ConfigurationManager.GameDir + @"\Data\interface.mpq", FileAccess.Read);
                misc = new MpqArchive(ConfigurationManager.GameDir + @"\Data\misc.mpq", FileAccess.Read);
                sound = new MpqArchive(ConfigurationManager.GameDir + @"\Data\sound.mpq", FileAccess.Read);
                terrain = new MpqArchive(ConfigurationManager.GameDir + @"\Data\terrain.mpq", FileAccess.Read);
                texture = new MpqArchive(ConfigurationManager.GameDir + @"\Data\texture.mpq", FileAccess.Read);
                wmo = new MpqArchive(ConfigurationManager.GameDir + @"\Data\wmo.mpq", FileAccess.Read);

            }

        }
        #endregion

        public static void GenerateMainListFileFromMPQ()
        {
            //Only generate list if empty
            if (MainListFile.Count == 0)
            {
                DirectoryInfo directory = new DirectoryInfo(ConfigurationManager.GameDir + "\\data\\");
                FileInfo[] Archives = directory.GetFiles("*.mpq", SearchOption.TopDirectoryOnly);
                string listFile;

                foreach (FileInfo fileinfo in Archives)
                {
                    try
                    {
                        using (MpqArchive archive = new MpqArchive(fileinfo.FullName, FileAccess.Read))
                        {
                            using (MpqFileStream file = archive.OpenFile("(listfile)"))
                            using (StreamReader sr = new StreamReader(file))
                            {
                                listFile = sr.ReadToEnd();
                                MainListFile.Add(listFile.ToLower() + ";" + fileinfo.Name);
                            }
                        }
                    }
                    catch
                    {
                        throw new Exception("Could not read: \"" + fileinfo.FullName + "\".");
                    }
                }

                //Add locale to the listfile:
                if (ConfigurationManager.Profile != 1) //Anything else than Vanilla
                {
                    using (MpqArchive archive = new MpqArchive(ConfigurationManager.GameDir + "\\data\\" + locale + "\\" + "locale-" + locale + ".mpq", FileAccess.Read))
                    {
                        using (MpqFileStream file = archive.OpenFile("(listfile)"))
                        using (StreamReader sr = new StreamReader(file))
                        {
                            listFile = sr.ReadToEnd();
                            MainListFile.Add(listFile.ToLower() + ";" + "locale1.txt"); //.txt because i need an extension, that's how i coded the damn thing, and that's how it will stay for the moment
                        }
                    }
                }

            }
        }

        public static Stream ReadThisFile(string filename)
        {
            Stream stream = null;
            if (usingCasc) //CASC
            {
                stream = cascHandler.OpenFile(filename);
            }
            else //MPQ
            {
                //Find the goddamn file in the archive hell
                int index = MainListFile.FindIndex(a => a.Contains(filename.ToLower()));

                if (index != -1)
                {
                    //Get the archive in which the requested file resides
                    string archive = MainListFile[index];
                    archive = archive.Substring(archive.LastIndexOf(';', archive.Length - 2) + 1);

                    switch (archive.Substring(0, archive.Length - 4).Replace("-", ""))
                    {
                        //Mysts
                        case "expansion4":
                            stream = expansion4.OpenFile(filename);
                            break;
                        case "model":
                            stream = model.OpenFile(filename);
                            break;

                        //Cataclysm
                        case "art":
                            stream = art.OpenFile(filename);
                            break;
                        case "expansion1":
                            stream = expansion1.OpenFile(filename);
                            break;
                        case "expansion2":
                            stream = expansion2.OpenFile(filename);
                            break;
                        case "expansion3":
                            stream = expansion3.OpenFile(filename);
                            break;
                        case "world":
                            stream = world.OpenFile(filename);
                            break;
                        case "world2":
                            stream = world2.OpenFile(filename);
                            break;

                        case "locale1":
                            stream = locale1.OpenFile(filename);
                            break;

                        //Lich King
                        case "common":
                            stream = common.OpenFile(filename);
                            break;
                        case "common2":
                            stream = common2.OpenFile(filename);
                            break;

                        case "expansion":
                            stream = expansion.OpenFile(filename);
                            break;
                        case "lichking":
                            stream = lichking.OpenFile(filename);
                            break;
                        case "patch":
                            stream = patch.OpenFile(filename);
                            break;
                        case "patch2":
                            stream = patch2.OpenFile(filename);
                            break;
                        case "patch3":
                            stream = patch3.OpenFile(filename);
                            break;

                        //Vanilla
                        case "base":
                            stream = baseMPQ.OpenFile(filename);
                            break;
                        case "dbc":
                            stream = dbc.OpenFile(filename);
                            break;
                        case "interface":
                            stream = interfaceMPQ.OpenFile(filename);
                            break;
                        case "misc":
                            stream = misc.OpenFile(filename);
                            break;
                        case "sound":
                            stream = sound.OpenFile(filename);
                            break;
                        case "terrain":
                            stream = terrain.OpenFile(filename);
                            break;
                        case "texture":
                            stream = texture.OpenFile(filename);
                            break;
                        case "wmo":
                            stream = wmo.OpenFile(filename);
                            break;
                    }
                }
                else
                {
                    //Missing file
                    stream = null;
                }
            }
            return stream;
        }

        public static void ExtractListfiles(string to)
        {
            DirectoryInfo directory = new DirectoryInfo(ConfigurationManager.GameDir + "\\data\\");
            FileInfo[] Archives = directory.GetFiles("*.mpq");
            foreach (FileInfo fileinfo in Archives)
            {
                if (!File.Exists(to + "\\" + fileinfo.Name.Replace(".MPQ", ".txt")))
                {
                    using (MpqArchive archive = new MpqArchive(fileinfo.FullName, FileAccess.Read))
                    {
                        archive.ExtractFile("(listfile)", to + "\\" + fileinfo.Name.Replace(".MPQ", ".txt"));
                    }
                }
            }

            if (ConfigurationManager.Profile != 1) //Anything else than vanilla
            {
                if (!File.Exists(to + "\\" + "locale1.txt"))
                {
                    using (MpqArchive archive = new MpqArchive(ConfigurationManager.GameDir + "\\data\\" + locale + "\\" + "locale-" + locale + ".mpq", FileAccess.Read))
                    {
                        archive.ExtractFile("(listfile)", to + "\\" + "locale1.txt");
                    }
                }
            }

        }

        public static Boolean FileExists(string filename)
        {
            bool exists;
            if (usingCasc)
            {
                exists = cascHandler.FileExists(filename);
            }

            else
            {
                if (MainListFile.FindIndex(a => a.Contains(filename.ToLower())) != -1) //It returns -1 if the file is not found
                {
                    exists = true;
                }
                else
                {
                    exists = false;
                }
            }
            return exists;
        }

        public static bool FileExists(uint fdid)
        {
            return cascHandler.FileExists((int)fdid);
        }

        public static void FindLocale()
        {
            //TODO: Find a way to get the locale of >WOD

            //deDE: German(Germany)
            //enGB: English(United Kingdom) - enGB clients return enUS
            //enUS: English(United States)
            //esES: Spanish(Spain)
            //esMX: Spanish(Mexico)
            //frFR: French(France)
            //koKR: Korean(Korea)
            //ptBR: Portuguese(Brazil)
            //ruRU: Russian(Russia) - UI AddOn
            //zhCN: Chinese(Simplified, PRC)
            //zhTW: Chinese(Traditional, Taiwan)

            var directories = Directory.GetDirectories(ConfigurationManager.GameDir + @"\Data\");
            foreach (string folder in directories)
            {
                switch (folder.Substring(folder.Length - 4, 4).ToLower())
                {
                    case "face": // "interFACE" that's why it's here
                        continue;
                    case "dede":
                        locale = "deDE";
                        break;
                    case "engb":
                        locale = "enGB";
                        break;
                    case "enus":
                        locale = "enUS";
                        break;
                    case "eses":
                        locale = "seES";
                        break;
                    case "esmx":
                        locale = "esMX";
                        break;
                    case "frfr":
                        locale = "frFR";
                        break;
                    case "kokr":
                        locale = "koKR";
                        break;
                    case "ptbr":
                        locale = "ptBR";
                        break;
                    case "ruru":
                        locale = "ruRU";
                        break;
                    case "zhcn":
                        locale = "zhCN";
                        break;
                    case "zhtw":
                        locale = "zhTW";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}