﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using CASCLib;

namespace OBJExporterUI.Exporters.OBJ
{
    public class ADTExporter
    {
        public static void exportADT(string file, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            var outdir = ConfigurationManager.AppSettings["outdir"];

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var TileSize = 1600.0f / 3.0f; //533.333
            var ChunkSize = TileSize / 16.0f; //33.333
            var UnitSize = ChunkSize / 8.0f; //4.166666

            var mapname = file.Replace("world/maps/", "").Substring(0, file.Replace("world/maps/", "").IndexOf("/"));
            var coord = file.Replace("world/maps/" + mapname + "/" + mapname, "").Replace(".adt", "").Split('_');

            Logger.WriteLine("ADT OBJ Exporter: Starting export of {0}..", file);

            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            exportworker.ReportProgress(0, "Loading ADT " + file);

            var reader = new ADTReader();
            reader.LoadADT(file.Replace('/', '\\'));

            if (reader.adtfile.chunks == null)
            {
                Logger.WriteLine("ADT OBJ Exporter: File {0} has no chunks, skipping export!", file);
                return;
            }

            var renderBatches = new List<Structs.RenderBatch>();
            var verticelist = new List<Structs.Vertex>();
            var indicelist = new List<int>();
            var materials = new Dictionary<int, string>();

            ConfigurationManager.RefreshSection("appSettings");
            var bakeQuality = ConfigurationManager.AppSettings["bakeQuality"];

            var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
            var initialChunkX = reader.adtfile.chunks[0].header.position.X;

            for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
            {
                var chunk = reader.adtfile.chunks[c];

                var off = verticelist.Count();

                var batch = new Structs.RenderBatch();

                for (int i = 0, idx = 0; i < 17; i++)
                {
                    for (var j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                    {
                        var v = new Structs.Vertex();
                        v.Normal = new Vector3(chunk.normals.normal_2[idx] / 127f, chunk.normals.normal_0[idx] / 127f, chunk.normals.normal_1[idx] / 127f);
                        v.Position = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, chunk.header.position.X - (i * UnitSize * 0.5f));
                        if ((i % 2) != 0) v.Position.X -= 0.5f * UnitSize;
                        if(bakeQuality == "low" || bakeQuality == "medium")
                        {
                            v.TexCoord = new Vector2(-(v.Position.X - initialChunkX) / TileSize, -(v.Position.Z - initialChunkY) / TileSize);
                        }
                        else if(bakeQuality == "high")
                        {
                            v.TexCoord = new Vector2(-(v.Position.X - initialChunkX) / ChunkSize, -(v.Position.Z - initialChunkY) / ChunkSize);
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
                        var currentHole = (int) Math.Pow (2,
                                Math.Floor (xx / 2f) * 1f +
                                Math.Floor (yy / 2f) * 4f);

                        // Check if current hole number should be a hole
                        if ((chunk.header.holesLowRes & currentHole) == 0)
                        {
                            isHole = false;
                        }
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
                        /*
                        indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j - 8 });
                        indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j + 8 });
                        */
                    }

                    if ((j + 1) % (9 + 8) == 0) j += 9;
                }

                if (bakeQuality == "low" || bakeQuality == "medium")
                {
                    if (!materials.ContainsKey(1))
                    {
                        materials.Add(1, Path.GetFileNameWithoutExtension(file));
                    }
                    batch.materialID = (uint)materials.Count();
                }
                else
                {
                    materials.Add((int)c + 1, Path.GetFileNameWithoutExtension(file) + "_" + c);
                    batch.materialID = c + 1;
                }

                batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                var layermats = new List<uint>();


                renderBatches.Add(batch);
            }
            ConfigurationManager.RefreshSection("appSettings");
            Console.WriteLine(ConfigurationManager.AppSettings["exportEverything"]);
            if(ConfigurationManager.AppSettings["exportEverything"] == "True")
            {
                var doodadSW = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + "_ModelPlacementInformation.csv"));
                doodadSW.WriteLine("ModelFile;PositionX;PositionY;PositionZ;RotationX;RotationY;RotationZ;ScaleFactor;ModelId;Type");

                exportworker.ReportProgress(25, "Exporting WMOs");

                for (var mi = 0; mi < reader.adtfile.objects.worldModels.entries.Count(); mi++)
                {
                    var wmo = reader.adtfile.objects.worldModels.entries[mi];
                    
                    var filename = reader.adtfile.objects.wmoNames.filenames[wmo.mwidEntry];

                    if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj")))
                    {
                        WMOExporter.exportWMO(filename, null, Path.Combine(outdir, Path.GetDirectoryName(file)), wmo.doodadSet);
                    }

                    doodadSW.WriteLine(Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj;" + wmo.position.X + ";" + wmo.position.Y + ";" + wmo.position.Z + ";" + wmo.rotation.X + ";" + wmo.rotation.Y + ";" + wmo.rotation.Z + ";" + wmo.scale / 1024f + ";" + wmo.uniqueId + ";wmo");
                }

                exportworker.ReportProgress(50, "Exporting M2s");

                for (var mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                {
                    var doodad = reader.adtfile.objects.models.entries[mi];

                    var filename = reader.adtfile.objects.m2Names.filenames[doodad.mmidEntry];

                    if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj")))
                    {
                        M2Exporter.exportM2(filename, null, Path.Combine(outdir, Path.GetDirectoryName(file)));
                    }

                    doodadSW.WriteLine(Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj;" + doodad.position.X + ";" + doodad.position.Y + ";" + doodad.position.Z + ";" + doodad.rotation.X + ";" + doodad.rotation.Y + ";" + doodad.rotation.Z + ";" + doodad.scale / 1024f + ";" + doodad.uniqueId + ";m2");
                }

                doodadSW.Close();
            }

            exportworker.ReportProgress(75, "Exporting terrain textures..");

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

            exportworker.ReportProgress(85, "Exporting terrain geometry..");

            var indices = indicelist.ToArray();

            var adtname = Path.GetFileNameWithoutExtension(file);

            var objsw = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".obj"));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl");
            objsw.WriteLine("g " + adtname.Replace(" ", ""));

            foreach (var vertex in verticelist)
            {
                objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Normal.X + " " + vertex.Normal.Y + " " + vertex.Normal.Z);
            }

            foreach (var renderBatch in renderBatches)
            {
                var i = renderBatch.firstFace;
                if (materials.ContainsKey((int)renderBatch.materialID)) { objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]); objsw.WriteLine("s 1"); }
                while (i < (renderBatch.firstFace + renderBatch.numFaces))
                {
                    objsw.WriteLine("f " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();
        }
    }
}
