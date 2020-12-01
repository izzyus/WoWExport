using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;

namespace Exporters.OBJ
{
    public class ADTExporter
    {
        public static void exportADT(string file, string outdir, string bakeQuality, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
            }

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var MaxSize = 51200 / 3.0;
            var TileSize = MaxSize / 32.0;
            var ChunkSize = TileSize / 16.0;
            var UnitSize = ChunkSize / 8.0;
            var UnitSizeHalf = UnitSize / 2.0;

            string mapname = file;
            mapname = mapname.Substring(mapname.LastIndexOf("\\", mapname.Length - 2) + 1);
            mapname = mapname.Substring(0, mapname.Length - 4);

            var reader = new ADTReader();

            var ADTfile = file;

            exportworker.ReportProgress(0, "Loading ADT " + file);

            if (Managers.ConfigurationManager.Profile <= 3) //WoTLK and below
            {
                reader.Load335ADT(ADTfile);
            }
            else
            {
                reader.LoadADT(ADTfile);
            }

            if (reader.adtfile.chunks == null)
            {
                throw new Exception("ADT OBJ Exporter: File has no chunks, skipping export!");
            }

            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            var renderBatches = new List<Structs.RenderBatch>();
            var verticelist = new List<Structs.Vertex>();
            var indicelist = new List<int>();
            var materials = new Dictionary<int, string>();

            // Calculate ADT offset in world coordinates
            var adtStartX = reader.adtfile.chunks[0].header.position.X;
            var adtStartY = reader.adtfile.chunks[0].header.position.Y;

            // Calculate first chunk offset in world coordinates
            var initialChunkX = adtStartX + (reader.adtfile.chunks[0].header.indexX * ChunkSize) * -1;
            var initialChunkY = adtStartY + (reader.adtfile.chunks[0].header.indexY * ChunkSize) * -1;

            uint ci = 0;
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    var genx = (initialChunkX + (ChunkSize * x) * -1);
                    var geny = (initialChunkY + (ChunkSize * y) * -1);

                    var chunk = reader.adtfile.chunks[ci];

                    var off = verticelist.Count();

                    var batch = new Structs.RenderBatch();

                    for (int row = 0, idx = 0; row < 17; row++)
                    {
                        bool isSmallRow = (row % 2) != 0;
                        int rowLength = isSmallRow ? 8 : 9;

                        for (var col = 0; col < rowLength; col++)
                        {
                            var v = new Structs.Vertex();

                            v.Normal = new Structs.Vector3D
                            {
                                X = (double)chunk.normals.normal_0[idx] / 127,
                                Y = (double)chunk.normals.normal_2[idx] / 127,
                                Z = (double)chunk.normals.normal_1[idx] / 127
                            };

                            var px = geny - (col * UnitSize);
                            var py = chunk.vertices.vertices[idx++] + chunk.header.position.Z;
                            var pz = genx - (row * UnitSizeHalf);

                            v.Position = new Structs.Vector3D
                            {
                                X = px,
                                Y = py,
                                Z = pz
                            };

                            if ((row % 2) != 0) v.Position.X = (px - UnitSizeHalf);

                            double ofs = col;
                            if (isSmallRow)
                                ofs += 0.5;

                            if (bakeQuality == "high")
                            {
                                double tx = ofs / 8d;
                                double ty = 1 - (row / 16d);
                                v.TexCoord = new Structs.Vector2D { X = tx, Y = ty };
                            }
                            else
                            {
                                double tx = -(v.Position.X - initialChunkY) / TileSize;
                                double ty = (v.Position.Z - initialChunkX) / TileSize;

                                v.TexCoord = new Structs.Vector2D { X = tx, Y = ty };
                            }
                            verticelist.Add(v);
                        }
                    }

                    batch.firstFace = (uint)indicelist.Count();

                    // Stupid C# and its structs
                    var holesHighRes = new byte[8];
                    holesHighRes[0] = chunk.header.holesHighRes_0;
                    holesHighRes[1] = chunk.header.holesHighRes_1;
                    holesHighRes[2] = chunk.header.holesHighRes_2;
                    holesHighRes[3] = chunk.header.holesHighRes_3;
                    holesHighRes[4] = chunk.header.holesHighRes_4;
                    holesHighRes[5] = chunk.header.holesHighRes_5;
                    holesHighRes[6] = chunk.header.holesHighRes_6;
                    holesHighRes[7] = chunk.header.holesHighRes_7;

                    for (int j = 9, xx = 0, yy = 0; j < 145; j++, xx++)
                    {
                        if (xx >= 8) { xx = 0; ++yy; }
                        var isHole = true;

                        // Check if chunk is using low-res holes
                        if ((chunk.header.flags & 0x10000) == 0)
                        {
                            // Calculate current hole number
                            var currentHole = (int)Math.Pow(2,
                                    Math.Floor(xx / 2f) * 1f +
                                    Math.Floor(yy / 2f) * 4f);

                            // Check if current hole number should be a hole
                            if ((chunk.header.holesLowRes & currentHole) == 0)
                            {
                                isHole = false;
                            }
                        }

                        //Sloppy ignore holes:
                        if (Managers.ConfigurationManager.ADTIgnoreHoles)
                        {
                            isHole = false;
                        }
                        else
                        {
                            // Check if current section is a hole
                            if (((holesHighRes[yy] >> xx) & 1) == 0)
                            {
                                isHole = false;
                            }
                        }

                        if (!isHole)
                        {
                            indicelist.AddRange(new int[] { off + j + 8, off + j - 9, off + j });
                            indicelist.AddRange(new int[] { off + j - 9, off + j - 8, off + j });
                            indicelist.AddRange(new int[] { off + j - 8, off + j + 9, off + j });
                            indicelist.AddRange(new int[] { off + j + 9, off + j + 8, off + j });

                            // Generates quads instead of 4x triangles
                            //indicelist.AddRange(new int[] { off + j + 8, off + j - 9, off + j - 8 });
                            //indicelist.AddRange(new int[] { off + j - 8, off + j + 9, off + j + 8 });
                        }
                        if ((j + 1) % (9 + 8) == 0) j += 9;
                    }

                    if (bakeQuality == "high")
                    {
                        materials.Add((int)ci + 1, Path.GetFileNameWithoutExtension(file) + "_" + ci);
                        batch.materialID = ci + 1;
                    }
                    else
                    {
                        if (!materials.ContainsKey(1))
                        {
                            materials.Add(1, Path.GetFileNameWithoutExtension(file));
                        }
                        batch.materialID = (uint)materials.Count();
                    }
                    batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                    renderBatches.Add(batch);
                    ci++;
                }
            }

            bool exportWMO = Managers.ConfigurationManager.ADTExportWMO;
            bool exportM2 = Managers.ConfigurationManager.ADTExportM2;
            bool exportTextures = Managers.ConfigurationManager.ADTexportTextures;
            bool exportAlphaMaps = Managers.ConfigurationManager.ADTexportAlphaMaps;
            bool exportHeightmap = Managers.ConfigurationManager.ADTExportHeightmap;
            bool exportFoliage = Managers.ConfigurationManager.ADTExportFoliage;

            //FOLIAGE
            if (exportFoliage)
            {
                exportworker.ReportProgress(65, "Exporting foliage");

                try
                {
                    var build = Managers.ArchiveManager.cascHandler.Config.VersionName; //TODO: IMPLEMENT PRE CASC BUILDS
                    var dbcd = new DBCD.DBCD(new WoWExport.DBC.ArchiveDBCProvider(), new WoWExport.DBC.LocalDBCDProvider());
                    var groundEffectTextureDB = dbcd.Load("GroundEffectTexture", build);
                    var groundEffectDoodadDB = dbcd.Load("GroundEffectDoodad", build);

                    for (var c = 0; c < reader.adtfile.texChunks.Length; c++)
                    {
                        for (var l = 0; l < reader.adtfile.texChunks[c].layers.Length; l++)
                        {
                            var effectID = reader.adtfile.texChunks[c].layers[l].effectId;
                            if (effectID == 0)
                                continue;

                            if (!groundEffectTextureDB.ContainsKey(effectID))
                            {
                                continue;
                            }

                            dynamic textureEntry = groundEffectTextureDB[effectID];
                            foreach (int doodad in textureEntry.DoodadID)
                            {
                                if (!groundEffectDoodadDB.ContainsKey(doodad))
                                {
                                    continue;
                                }

                                dynamic doodadEntry = groundEffectDoodadDB[doodad];

                                var filedataid = (uint)doodadEntry.ModelFileID;

                                if (!WoWExport.Listfile.TryGetFilename(filedataid, out var filename))
                                {
                                    Console.WriteLine("Could not find filename for " + filedataid + ", setting filename to filedataid..");
                                    filename = filedataid.ToString();
                                }

                                if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), "foliage")))
                                {
                                    Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file), "foliage"));
                                }

                                if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), "foliage", Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj")))
                                {
                                    M2Exporter.ExportM2(filename, Path.Combine(outdir, Path.GetDirectoryName(file), "foliage"));
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error exporting GroundEffects: " + e.Message);
                }
            }

            if (exportWMO || exportM2)
            {
                var doodadSW = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + "_ModelPlacementInformation.csv"));
                doodadSW.WriteLine("ModelFile;PositionX;PositionY;PositionZ;RotationX;RotationY;RotationZ;ScaleFactor;ModelId;Type");

                if (exportWMO)
                {
                    exportworker.ReportProgress(25, "Exporting WMOs");

                    for (var mi = 0; mi < reader.adtfile.objects.worldModels.entries.Count(); mi++)
                    {
                        var wmo = reader.adtfile.objects.worldModels.entries[mi];

                        float wmoScale;
                        if (wmo.scale != 0)
                        {
                            wmoScale = wmo.scale / 1024f;
                        }
                        else
                        {
                            wmoScale = 1;
                        }

                        var filename = reader.adtfile.objects.wmoNames.filenames[wmo.mwidEntry];

                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj")))
                        {
                            WMOExporter.ExportWMO(filename, Path.Combine(outdir, Path.GetDirectoryName(file)), exportworker, null, wmo.doodadSet);
                        }

                        if (Managers.ConfigurationManager.ADTModelsPlacementGlobalPath)
                        {
                            doodadSW.WriteLine(Path.Combine(Path.GetDirectoryName(filename).ToLower(), Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj") + ";" + wmo.position.X + ";" + wmo.position.Y + ";" + wmo.position.Z + ";" + wmo.rotation.X + ";" + wmo.rotation.Y + ";" + wmo.rotation.Z + ";" + wmoScale + ";" + wmo.uniqueId + ";wmo");
                        }
                        else
                        {
                            doodadSW.WriteLine(Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj;" + wmo.position.X + ";" + wmo.position.Y + ";" + wmo.position.Z + ";" + wmo.rotation.X + ";" + wmo.rotation.Y + ";" + wmo.rotation.Z + ";" + wmoScale + ";" + wmo.uniqueId + ";wmo");
                        }
                    }
                }

                if (exportM2)
                {
                    exportworker.ReportProgress(50, "Exporting M2s");

                    for (var mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                    {
                        var doodad = reader.adtfile.objects.models.entries[mi];

                        string filename;
                        filename = reader.adtfile.objects.m2Names.filenames[doodad.mmidEntry];
                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj")))
                        {
                            M2Exporter.ExportM2(filename, Path.Combine(outdir, Path.GetDirectoryName(file)), exportworker);
                        }
                        if (Managers.ConfigurationManager.ADTModelsPlacementGlobalPath)
                        {
                            doodadSW.WriteLine(Path.Combine(Path.GetDirectoryName(filename).ToLower(), Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj") + ";" + doodad.position.X + ";" + doodad.position.Y + ";" + doodad.position.Z + ";" + doodad.rotation.X + ";" + doodad.rotation.Y + ";" + doodad.rotation.Z + ";" + doodad.scale / 1024f + ";" + doodad.uniqueId + ";m2");
                        }
                        else
                        {
                            doodadSW.WriteLine(Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj;" + doodad.position.X + ";" + doodad.position.Y + ";" + doodad.position.Z + ";" + doodad.rotation.X + ";" + doodad.rotation.Y + ";" + doodad.rotation.Z + ";" + doodad.scale / 1024f + ";" + doodad.uniqueId + ";m2");
                        }
                    }
                }
                doodadSW.Close();
            }
            #region Alpha&Tex
            //----------------------------------------------------------------------------------------------------------
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///TEXTURE & ALPHA & HEIGHTMAP RELATED START
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //----------------------------------------------------------------------------------------------------------
            if (exportTextures) //Export ground textures
            {
                {
                    if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures")))
                    {
                        Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures"));
                    }
                }

                List<String> GroundTextures = reader.adtfile.textures.filenames.ToList();

                var blpreader = new BLPReader();
                foreach (string texture in GroundTextures)
                {
                    if (Managers.ConfigurationManager.ADTPreserveTextureStruct)
                    {
                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures", Path.GetDirectoryName(texture) + Path.GetFileNameWithoutExtension(texture) + ".png")))
                        {
                            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures", Path.GetDirectoryName(texture))))
                            {
                                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures", Path.GetDirectoryName(texture)));
                            }

                            if (Managers.ArchiveManager.FileExists(texture))
                            {
                                try
                                {
                                    blpreader.LoadBLP(Managers.ArchiveManager.ReadThisFile(texture));
                                    blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures", Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + ".png"));
                                    if (Managers.ConfigurationManager.ADTExportSpecularTextures)
                                    {
                                        if (Managers.ArchiveManager.FileExists(Path.Combine(Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + "_s.blp")))
                                        {
                                            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures", Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + "_s.png")))
                                            {
                                                blpreader.LoadBLP(Managers.ArchiveManager.ReadThisFile(Path.Combine(Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + "_s.blp")));
                                                blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures", Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + "_s.png"));
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    //Error on file save
                                    throw new Exception(e.Message);
                                }
                            }
                            else
                            {
                                //Missing file
                            }
                        }
                    }
                    else
                    {
                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures\\", Path.GetFileNameWithoutExtension(texture) + ".png")))
                        {
                            if (Managers.ArchiveManager.FileExists(texture))
                            {
                                try
                                {
                                    blpreader.LoadBLP(Managers.ArchiveManager.ReadThisFile(texture));
                                    blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures\\", Path.GetFileNameWithoutExtension(texture) + ".png"));
                                    if (Managers.ConfigurationManager.ADTExportSpecularTextures)
                                    {
                                        if (Managers.ArchiveManager.FileExists(Path.Combine(Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + "_s.blp")))
                                        {
                                            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures\\", Path.GetFileNameWithoutExtension(texture) + "_s.png")))
                                            {
                                                blpreader.LoadBLP(Managers.ArchiveManager.ReadThisFile(Path.Combine(Path.GetDirectoryName(texture), Path.GetFileNameWithoutExtension(texture) + "_s.blp")));
                                                blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\GroundTextures\\", Path.GetFileNameWithoutExtension(texture) + "_s.png"));
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    //Error on file save
                                    throw new Exception(e.Message);
                                }
                            }
                            else
                            {
                                //Missing file
                            }
                        }
                    }
                }
            }

            if (exportAlphaMaps)
            {
                Generators.ADT_Alpha.ADT_Alpha AlphaMapsGenerator = new Generators.ADT_Alpha.ADT_Alpha();
                AlphaMapsGenerator.GenerateAlphaMaps(reader.adtfile, Managers.ConfigurationManager.ADTAlphaMode);

                List<System.Drawing.Bitmap> AlphaLayers = new List<System.Drawing.Bitmap>();
                AlphaLayers = AlphaMapsGenerator.AlphaLayers;

                if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps")))
                {
                    Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps"));
                }

                if (AlphaLayers != null)
                {
                    if (Managers.ConfigurationManager.ADTAlphaMode == 0 || Managers.ConfigurationManager.ADTAlphaMode == 1)
                    {
                        for (int m = 0; m < AlphaLayers.ToArray().Length; m++)
                        {
                            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_" + m + ".png")))
                            {
                                try
                                {
                                    AlphaLayers[m].Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_" + m + ".png"));
                                }
                                catch
                                {
                                    //Error on file save
                                }
                            }
                        }
                    }
                    if (Managers.ConfigurationManager.ADTAlphaMode == 2 || Managers.ConfigurationManager.ADTAlphaMode == 3)
                    {
                        List<String> AlphaLayersNames = new List<String>(AlphaMapsGenerator.AlphaLayersNames);
                        for (int m = 0; m < AlphaLayers.ToArray().Length; m++)
                        {
                            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_" + AlphaLayersNames[m].Replace(";", "_") + ".png")))
                            {
                                try
                                {
                                    AlphaLayers[m].Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_" + AlphaLayersNames[m].Replace(";", "_") + ".png"));
                                }
                                catch
                                {
                                    //Error on file save
                                }
                            }
                        }
                    }
                    if (Managers.ConfigurationManager.ADTAlphaMode == 4) //Splatmaps
                    {
                        //Save the splatmaps
                        for (int m = 0; m < AlphaLayers.ToArray().Length; m++)
                        {
                            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_splatmap_" + m + ".png")))
                            {
                                try
                                {
                                    AlphaLayers[m].Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_splatmap_" + m + ".png"));
                                }
                                catch
                                {
                                    //Error on file save
                                }
                            }
                        }
                        //Save the material info JSON
                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_MaterialData.json")))
                        {
                            try
                            {
                                File.WriteAllText(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\AlphaMaps\\", mapname + "_MaterialData.json"), AlphaMapsGenerator.SplatmapJSON);
                            }
                            catch
                            {
                                //Error on file save
                            }
                        }

                    }
                }

                //Check if the CSV already exists, if not, create it
                if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\" + mapname + "_" + "layers.csv")))
                {
                    //Generate layer information CSV
                    StreamWriter layerCsvSR = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\" + mapname + "_" + "layers.csv"));
                    //Insert CSV scheme
                    layerCsvSR.WriteLine("chunk;tex0;tex1;tex2;tex3");
                    for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                    {
                        string csvLine = c.ToString();
                        for (int li = 0; li < reader.adtfile.texChunks[c].layers.Count(); li++)
                        {
                            var AlphaLayerName = reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[li].textureId].ToLower();
                            if (Managers.ConfigurationManager.ADTPreserveTextureStruct)
                            {
                                csvLine = csvLine + ";" + Path.Combine(Path.GetDirectoryName(AlphaLayerName), Path.GetFileNameWithoutExtension(AlphaLayerName));
                            }
                            else
                            {
                                csvLine = csvLine + ";" + Path.GetFileNameWithoutExtension(AlphaLayerName);
                            }

                        }
                        layerCsvSR.WriteLine(csvLine);
                    }
                    layerCsvSR.Close();
                }
            }

            if (exportHeightmap)
            {
                Generators.ADT_Height.ADT_Height heightmapGenerator = new Generators.ADT_Height.ADT_Height();
                heightmapGenerator.GenerateHeightmap(reader.adtfile);

                if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\HeightMaps")))
                {
                    Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\HeightMaps"));
                }

                try
                {
                    File.WriteAllText(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\HeightMaps\\", mapname + "_HeightData.json"), Newtonsoft.Json.JsonConvert.SerializeObject(heightmapGenerator.heightArray2d));
                    heightmapGenerator.heightMap.Save(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\HeightMaps\\", mapname + "_heightmap.png"));
                }
                catch
                {
                    //Error on save
                }
            }
            //----------------------------------------------------------------------------------------------------------
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///TEXTURE & ALPHA & HEIGHTMAP RELATED END
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //----------------------------------------------------------------------------------------------------------
            #endregion

            //VERTEX COLORS -- not implemented
            /*
            //Vertex color data
            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\" + mapname + "_" + "vertex_colors.csv")))
            {
                StreamWriter vertesColorCSV = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file) + "\\" + mapname + "_" + "vertex_colors.csv"));
                vertesColorCSV.WriteLine("chunk;vert;a;r;g;b"); //header
                for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                {
                    if (reader.adtfile.chunks[c].vertexShading.red != null)
                    {
                        for (int i = 0; i < 145; i++)
                        {
                            //Console.WriteLine(c + "_" + i + "-" + reader.adtfile.chunks[c].vertexShading.alpha[i] + " " + reader.adtfile.chunks[c].vertexShading.red[i] + " " + reader.adtfile.chunks[c].vertexShading.green[i] + " " + reader.adtfile.chunks[c].vertexShading.blue[i]);
                            vertesColorCSV.WriteLine(c + ";" + i + ";" + reader.adtfile.chunks[c].vertexShading.alpha[i] + ";" + reader.adtfile.chunks[c].vertexShading.red[i] + ";" + reader.adtfile.chunks[c].vertexShading.green[i] + ";" + reader.adtfile.chunks[c].vertexShading.blue[i]);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(c + "- null");
                        vertesColorCSV.WriteLine(c + ";0;0;0;0;0");
                    }
                }
                vertesColorCSV.Close();
            }
            */

            exportworker.ReportProgress(75, "Exporting terrain textures..");

            if (bakeQuality != "none")
            {
                var mtlsw = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl"));

                //No idea how MTL files really work yet. Needs more investigation.
                foreach (var material in materials)
                {
                    mtlsw.WriteLine("newmtl " + material.Value.Replace(" ", ""));
                    mtlsw.WriteLine("Ka 1.000000 1.000000 1.000000");
                    mtlsw.WriteLine("Kd 0.640000 0.640000 0.640000");
                    mtlsw.WriteLine("map_Ka " + material.Value.Replace(" ", "") + ".png");
                    mtlsw.WriteLine("map_Kd " + material.Value.Replace(" ", "") + ".png");
                }

                mtlsw.Close();
            }

            exportworker.ReportProgress(85, "Exporting terrain geometry..");

            var indices = indicelist.ToArray();

            var adtname = Path.GetFileNameWithoutExtension(file);

            var objsw = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".obj"));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            if (bakeQuality != "none")
            {
                objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl");
            }

            objsw.WriteLine("g " + adtname.Replace(" ", ""));
            var verticeCounter = 1;
            var chunkCounter = 1;
            foreach (var vertex in verticelist)
            {
                objsw.WriteLine("# C" + chunkCounter + ".V" + verticeCounter);
                objsw.WriteLine("v " + vertex.Position.X.ToString("R") + " " + vertex.Position.Y.ToString("R") + " " + vertex.Position.Z.ToString("R"));
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Normal.X.ToString("R") + " " + vertex.Normal.Y.ToString("R") + " " + vertex.Normal.Z.ToString("R"));
                verticeCounter++;
                if (verticeCounter == 146)
                {
                    chunkCounter++;
                    verticeCounter = 1;
                }
            }

            if (bakeQuality != "high")
            {
                objsw.WriteLine("usemtl " + materials[1]);
                objsw.WriteLine("s 1");
            }
            int currentChunk = 0;
            foreach (var renderBatch in renderBatches)
            {
                var i = renderBatch.firstFace;
                if (bakeQuality == "high" && materials.ContainsKey((int)renderBatch.materialID))
                {
                    if (Managers.ConfigurationManager.ADTSplitChunks)
                    {
                        objsw.WriteLine("g " + materials[(int)renderBatch.materialID]);
                        objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]);
                        objsw.WriteLine("s 1");
                    }
                    else
                    {
                        objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]);
                    }
                }
                while (i < (renderBatch.firstFace + renderBatch.numFaces))
                {
                    objsw.WriteLine("f " +
                        (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + " " +
                        (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " +
                        (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1));
                    i += 3;
                }
                currentChunk++;
            }

            objsw.Close();
        }
    }
}
