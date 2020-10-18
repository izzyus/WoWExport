using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.FileReaders;

namespace Exporters.OBJ
{
    public class WMOExporter
    {
        public static void ExportWMO(string filename, string outdir, BackgroundWorker exportworker = null, string destinationOverride = null, ushort doodadSetExportID = ushort.MaxValue)
        {
            filename = filename.ToLower();


            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            exportworker.ReportProgress(5, "Reading WMO..");

            var wmo = new WMOReader();
            wmo.LoadWMO(filename);


            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            exportworker.ReportProgress(30, "Reading WMO..");

            uint totalVertices = 0;

            var groups = new Structs.WMOGroup[wmo.wmofile.group.Count()];

            for (var g = 0; g < wmo.wmofile.group.Count(); g++)
            {
                if (wmo.wmofile.group[g].mogp.vertices == null)
                {
                    continue;
                }
                for (var i = 0; i < wmo.wmofile.groupNames.Count(); i++)
                {
                    if (wmo.wmofile.group[g].mogp.nameOffset == wmo.wmofile.groupNames[i].offset)
                    {
                        groups[g].name = wmo.wmofile.groupNames[i].name.Replace(" ", "_");
                    }
                }

                if (groups[g].name == "antiportal")
                {
                    //Console.WriteLine("Group is antiportal");
                    continue;
                }

                groups[g].verticeOffset = totalVertices;
                groups[g].vertices = new Structs.Vertex[wmo.wmofile.group[g].mogp.vertices.Count()];

                for (var i = 0; i < wmo.wmofile.group[g].mogp.vertices.Count(); i++)
                {
                    groups[g].vertices[i].Position = new Structs.Vector3D()
                    {
                        X = wmo.wmofile.group[g].mogp.vertices[i].vector.X * -1,
                        Y = wmo.wmofile.group[g].mogp.vertices[i].vector.Z,
                        Z = wmo.wmofile.group[g].mogp.vertices[i].vector.Y
                    };

                    groups[g].vertices[i].Normal = new Structs.Vector3D()
                    {
                        X = wmo.wmofile.group[g].mogp.normals[i].normal.X,
                        Y = wmo.wmofile.group[g].mogp.normals[i].normal.Z,
                        Z = wmo.wmofile.group[g].mogp.normals[i].normal.Y
                    };

                    groups[g].vertices[i].TexCoord = new Structs.Vector2D()
                    {
                        X = wmo.wmofile.group[g].mogp.textureCoords[0][i].X,
                        Y = wmo.wmofile.group[g].mogp.textureCoords[0][i].Y
                    };

                    totalVertices++;
                }

                var indicelist = new List<uint>();

                for (var i = 0; i < wmo.wmofile.group[g].mogp.indices.Count(); i++)
                {
                    indicelist.Add(wmo.wmofile.group[g].mogp.indices[i].indice);
                }

                groups[g].indices = indicelist.ToArray();
            }

            if (destinationOverride == null)
            {
                // Create output directory
                if (!string.IsNullOrEmpty(filename))
                {
                    if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename))))
                    {
                        Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(filename)));
                    }
                }
                else
                {
                    if (!Directory.Exists(outdir))
                    {
                        Directory.CreateDirectory(outdir);
                    }
                }
            }
            #region M2Export
            bool exportM2 = Managers.ConfigurationManager.WMOExportM2;
            if (exportM2)
            {

                StreamWriter doodadSW;

                if (destinationOverride == null)
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        doodadSW = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename.Replace(" ", "")) + "_ModelPlacementInformation.csv"));
                    }
                    else
                    {
                        doodadSW = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(filename), filename + "_ModelPlacementInformation.csv"));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        doodadSW = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileNameWithoutExtension(filename).Replace(" ", "") + "_ModelPlacementInformation.csv"));
                    }
                    else
                    {
                        doodadSW = new StreamWriter(Path.Combine(outdir, destinationOverride, filename + "_ModelPlacementInformation.csv"));
                    }
                }

                exportworker.ReportProgress(55, "Exporting doodads..");

                doodadSW.WriteLine("ModelFile;PositionX;PositionY;PositionZ;RotationW;RotationX;RotationY;RotationZ;ScaleFactor;DoodadSet");

                for (var i = 0; i < wmo.wmofile.doodadSets.Count(); i++)
                {
                    var doodadSet = wmo.wmofile.doodadSets[i];

                    var currentDoodadSetName = doodadSet.setName.Replace("Set_", "").Replace("SET_", "").Replace("$DefaultGlobal", "Default");

                    if (doodadSetExportID != ushort.MaxValue)
                    {
                        if (i != 0 && i != doodadSetExportID)
                        {
                            //Console.WriteLine("Skipping doodadset with ID " + i + " (" + currentDoodadSetName + ") because export filter is set to " + doodadSetExportID);
                            continue;
                        }
                    }

                    //Console.WriteLine("At doodadset " + i + " (" + currentDoodadSetName + ")");

                    for (var j = doodadSet.firstInstanceIndex; j < (doodadSet.firstInstanceIndex + doodadSet.numDoodads); j++)
                    {
                        foreach (var doodadNameEntry in wmo.wmofile.doodadNames)
                        {
                            var doodadDefinition = wmo.wmofile.doodadDefinitions[j];

                            if (doodadNameEntry.startOffset == doodadDefinition.offset)
                            {
                                var doodadFileName = doodadNameEntry.filename.Replace(".MDX", ".M2").Replace(".MDL", ".M2");

                                if (destinationOverride == null)
                                {
                                    if (Managers.ConfigurationManager.WMODoodadsGlobalPath)
                                    {
                                        if (!File.Exists(Path.Combine(outdir, Path.GetFileName(doodadFileName.ToLower()).Replace(".m2", ".obj"))))
                                        {

                                            M2Exporter.ExportM2(doodadFileName, outdir, exportworker);
                                        }
                                    }
                                    else
                                    {
                                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename), Path.GetFileName(doodadFileName.ToLower()).Replace(".m2", ".obj"))))
                                        {
                                            M2Exporter.ExportM2(doodadFileName, Path.Combine(outdir, Path.GetDirectoryName(filename)), exportworker);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!File.Exists(Path.Combine(destinationOverride, Path.GetFileName(doodadFileName.ToLower()).Replace(".m2", ".obj"))))
                                    {
                                        M2Exporter.ExportM2(doodadNameEntry.filename.Replace(".MDX", ".M2").Replace(".MDL", ".M2"), destinationOverride, exportworker);
                                    }
                                }

                                if (Managers.ConfigurationManager.WMODoodadsPlacementGlobalPath)
                                {
                                    doodadSW.WriteLine(doodadNameEntry.filename.ToLower().Replace(".mdx", ".m2").Replace(".mdl", ".m2").Replace(".m2", ".obj;") + doodadDefinition.position.X.ToString("F09") + ";" + doodadDefinition.position.Y.ToString("F09") + ";" + doodadDefinition.position.Z.ToString("F09") + ";" + doodadDefinition.rotation.W.ToString("F15") + ";" + doodadDefinition.rotation.X.ToString("F15") + ";" + doodadDefinition.rotation.Y.ToString("F15") + ";" + doodadDefinition.rotation.Z.ToString("F15") + ";" + doodadDefinition.scale + ";" + currentDoodadSetName);
                                }
                                else
                                {
                                    doodadSW.WriteLine(Path.GetFileNameWithoutExtension(doodadNameEntry.filename).ToLower() + ".obj;" + doodadDefinition.position.X.ToString("F09") + ";" + doodadDefinition.position.Y.ToString("F09") + ";" + doodadDefinition.position.Z.ToString("F09") + ";" + doodadDefinition.rotation.W.ToString("F15") + ";" + doodadDefinition.rotation.X.ToString("F15") + ";" + doodadDefinition.rotation.Y.ToString("F15") + ";" + doodadDefinition.rotation.Z.ToString("F15") + ";" + doodadDefinition.scale + ";" + currentDoodadSetName);
                                }

                                break;
                            }
                        }
                    }
                }
                doodadSW.Close();
            }
            #endregion
            var mtlsb = new StringBuilder();
            var textureID = 0;

            if (wmo.wmofile.materials == null)
            {
                return;
            }

            var materials = new Structs.Material[wmo.wmofile.materials.Count()];

            for (var i = 0; i < wmo.wmofile.materials.Count(); i++)
            {
                for (var ti = 0; ti < wmo.wmofile.textures.Count(); ti++)
                {
                    if (wmo.wmofile.textures[ti].startOffset == wmo.wmofile.materials[i].texture1)
                    {
                        materials[i].textureID = textureID + i;
                        materials[i].filename = Path.GetFileNameWithoutExtension(wmo.wmofile.textures[ti].filename);

                        if (wmo.wmofile.materials[i].blendMode == 0)
                        {
                            materials[i].transparent = false;
                        }
                        else
                        {
                            materials[i].transparent = true;
                        }

                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename), materials[i].filename + ".png")))
                        {
                            var blpreader = new BLPReader();
                            blpreader.LoadBLP(Managers.ArchiveManager.ReadThisFile(wmo.wmofile.textures[ti].filename));

                            try
                            {
                                if (destinationOverride == null)
                                {
                                    blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(filename), materials[i].filename + ".png"));
                                }
                                else
                                {
                                    blpreader.bmp.Save(Path.Combine(outdir, destinationOverride, materials[i].filename.ToLower() + ".png"));
                                }
                            }
                            catch
                            {
                                //Error on file save
                            }
                        }

                        textureID++;
                    }
                }
            }

            //No idea how MTL files really work yet. Needs more investigation.
            foreach (var material in materials)
            {
                mtlsb.Append("newmtl " + material.filename + "\n");
                mtlsb.Append("Ns 96.078431\n");
                mtlsb.Append("Ka 1.000000 1.000000 1.000000\n");
                mtlsb.Append("Kd 0.640000 0.640000 0.640000\n");
                mtlsb.Append("Ks 0.000000 0.000000 0.000000\n");
                mtlsb.Append("Ke 0.000000 0.000000 0.000000\n");
                mtlsb.Append("Ni 1.000000\n");
                mtlsb.Append("d 1.000000\n");
                mtlsb.Append("illum 1\n");
                mtlsb.Append("map_Kd " + material.filename + ".png\n");
                if (material.transparent)
                {
                    mtlsb.Append("map_d " + material.filename + ".png\n");
                }
                /* //temporary removed
                if (ConfigurationManager.AppSettings["textureMetadata"] == "True")
                {
                    mtlsb.Append("blend " + material.blendMode + "\n");
                    mtlsb.Append("shader " + material.shaderID + "\n");
                    mtlsb.Append("terrain " + material.terrainType + "\n");
                }
                */
            }

            if (!string.IsNullOrEmpty(filename))
            {
                if (destinationOverride == null)
                {
                    File.WriteAllText(Path.Combine(outdir, filename.Replace(".wmo", ".mtl")), mtlsb.ToString());
                }
                else
                {
                    File.WriteAllText(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".wmo", ".mtl")), mtlsb.ToString());
                }
            }
            else
            {
                if (destinationOverride == null)
                {
                    File.WriteAllText(Path.Combine(outdir, filename + ".mtl"), mtlsb.ToString());
                }
                else
                {
                    File.WriteAllText(Path.Combine(outdir, destinationOverride, filename + ".mtl"), mtlsb.ToString());
                }
            }

            exportworker.ReportProgress(75, "Exporting model..");

            var numRenderbatches = 0;
            //Get total amount of render batches
            for (var i = 0; i < wmo.wmofile.group.Count(); i++)
            {
                if (wmo.wmofile.group[i].mogp.renderBatches == null)
                {
                    continue;
                }
                numRenderbatches = numRenderbatches + wmo.wmofile.group[i].mogp.renderBatches.Count();
            }


            var rb = 0;
            for (var g = 0; g < wmo.wmofile.group.Count(); g++)
            {
                groups[g].renderBatches = new Structs.RenderBatch[numRenderbatches];

                var group = wmo.wmofile.group[g];
                if (group.mogp.renderBatches == null)
                {
                    continue;
                }

                for (var i = 0; i < group.mogp.renderBatches.Count(); i++)
                {
                    var batch = group.mogp.renderBatches[i];

                    groups[g].renderBatches[rb].firstFace = batch.firstFace;
                    groups[g].renderBatches[rb].numFaces = batch.numFaces;

                    if (batch.flags == 2)
                    {
                        groups[g].renderBatches[rb].materialID = (uint)batch.possibleBox2_3;
                    }
                    else
                    {
                        groups[g].renderBatches[rb].materialID = batch.materialID;
                    }
                    groups[g].renderBatches[rb].blendType = wmo.wmofile.materials[batch.materialID].blendMode;
                    groups[g].renderBatches[rb].groupID = (uint)g;
                    rb++;
                }
            }

            exportworker.ReportProgress(95, "Writing files..");

            StreamWriter objsw;
            if (!string.IsNullOrEmpty(filename))
            {
                if (destinationOverride == null)
                {
                    objsw = new StreamWriter(Path.Combine(outdir, filename.Replace(".wmo", ".obj")));
                }
                else
                {
                    objsw = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".wmo", ".obj")));
                }

                objsw.WriteLine("# Written by Marlamin's WoW Export Tools. Original file: " + filename);
                objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");
            }
            else
            {
                if (destinationOverride == null)
                {
                    objsw = new StreamWriter(Path.Combine(outdir, filename + ".obj"));
                }
                else
                {
                    objsw = new StreamWriter(Path.Combine(outdir, destinationOverride, filename + ".obj"));
                }
                objsw.WriteLine("# Written by Marlamin's WoW Export Tools. Original file id: " + filename);
                objsw.WriteLine("mtllib " + filename + ".mtl");
            }

            foreach (var group in groups)
            {
                if (group.vertices == null)
                {
                    continue;
                }
                //Console.WriteLine("Writing " + group.name);
                objsw.WriteLine("o " + group.name);

                //Added thunderysteak's adjustment (original commit: ed067c7c6e8321c33ef0f3679d33c9c472dcefc3)
                foreach (var vertex in group.vertices)
                {
                    objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                }

                foreach (var vertex in group.vertices)
                {
                    objsw.WriteLine("vt " + vertex.TexCoord.X + " " + (vertex.TexCoord.Y - 1) * -1);

                }
                foreach (var vertex in group.vertices)
                {
                    objsw.WriteLine("vn " + (-vertex.Normal.X).ToString("F12") + " " + vertex.Normal.Y.ToString("F12") + " " + vertex.Normal.Z.ToString("F12"));
                }

                var indices = group.indices;

                foreach (var renderbatch in group.renderBatches)
                {
                    var i = renderbatch.firstFace;
                    if (renderbatch.numFaces > 0)
                    {
                        //thunderysteak's adjustment
                        //objsw.WriteLine("o " + group.name); //?
                        objsw.WriteLine("g " + group.name);//3DS Max's OBJ importer fails with invalid normal index without groups being defined
                        //--------------------------
                        objsw.WriteLine("usemtl " + materials[renderbatch.materialID].filename);
                        objsw.WriteLine("s 1");
                        while (i < (renderbatch.firstFace + renderbatch.numFaces))
                        {
                            objsw.WriteLine("f " + (indices[i] + group.verticeOffset + 1) + "/" + (indices[i] + group.verticeOffset + 1) + "/" + (indices[i] + group.verticeOffset + 1) + " " + (indices[i + 1] + group.verticeOffset + 1) + "/" + (indices[i + 1] + group.verticeOffset + 1) + "/" + (indices[i + 1] + group.verticeOffset + 1) + " " + (indices[i + 2] + group.verticeOffset + 1) + "/" + (indices[i + 2] + group.verticeOffset + 1) + "/" + (indices[i + 2] + group.verticeOffset + 1));
                            i += 3;
                        }
                    }
                }
            }
            objsw.Close();
            //Console.WriteLine("Done loading WMO file!");
        }
    }
}
