//Experimental SMD module
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WoWFormatLib.FileReaders;
using System.IO;

namespace Exporters.SMD
{
    class M2SmdExporter
    {
        public static void ExportM2(string filename, string outdir, string destinationOverride = null)
        {
            filename = filename.ToLower();

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var reader = new M2Reader();

            //If the missing file is an ".mdx", try to look for a ".m2" alternative
            if (!Managers.ArchiveManager.FileExists(filename))
            {
                if (Path.GetExtension(filename) == ".mdx")
                {
                    if (!Managers.ArchiveManager.FileExists(filename.Replace(".mdx", ".m2")))
                    {
                        throw new Exception("404 M2 not found!");
                    }
                    else
                    {
                        filename = filename.Replace(".mdx", ".m2");
                    }
                }
                else
                {
                    throw new Exception("404 M2 not found!");
                }
            }

            reader.LoadM2(filename);

            //Bail if the model has no vertices
            if (reader.model.vertices.Count() == 0)
            {
                return;
            }
            
            //----------------------------------------------------------------------------------------------------------
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///REFERENCE SMD
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //----------------------------------------------------------------------------------------------------------
            StreamWriter smdsw;
            if (destinationOverride == null)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename))))
                    {
                        Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(filename)));
                    }
                    smdsw = new StreamWriter(Path.Combine(outdir, filename.Replace(".m2", ".smd")));
                }
                else
                {
                    if (!Directory.Exists(outdir))
                    {
                        Directory.CreateDirectory(outdir);
                    }
                    smdsw = new StreamWriter(Path.Combine(outdir, filename + ".smd"));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    smdsw = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".m2", ".smd")));
                }
                else
                {
                    smdsw = new StreamWriter(Path.Combine(outdir, destinationOverride, filename + ".smd"));
                }
            }

            smdsw.WriteLine("// Written by izzy's SMD Export module. Original file: " + filename);
            //Version
            smdsw.WriteLine("version 1");

            //Bones block start
            smdsw.WriteLine("nodes");

            //Root bone
            smdsw.WriteLine("  0 \"" + reader.model.name + "\"  -1");
            for (int i = 0; i < reader.model.bones.Count(); i++)
            {
                smdsw.WriteLine("  " + (i + 1) + " \"" + reader.model.name + "_bone" + i + "\"  " + (reader.model.bones[i].parentBone + 1));
            }
            smdsw.WriteLine("end");
            //Bones block end
            //Skeleton block start
            smdsw.WriteLine("skeleton");
            smdsw.WriteLine("time 0");

            smdsw.WriteLine("  0 0.000000 0.000000 0.000000 0.000000 -0.000000 0.000000");
            for (int i = 0; i < reader.model.bones.Count(); i++)
            {
                var currentBone = reader.model.bones[i];
                var currentBoneParentPos = new WoWFormatLib.Utils.Vector3();

                if (currentBone.parentBone != -1)
                {
                    currentBoneParentPos.X = reader.model.bones[reader.model.bones[i].parentBone].pivot.X;
                    currentBoneParentPos.Y = reader.model.bones[reader.model.bones[i].parentBone].pivot.Y;
                    currentBoneParentPos.Z = reader.model.bones[reader.model.bones[i].parentBone].pivot.Z;
                }
                else
                {
                    currentBoneParentPos.X = 0;
                    currentBoneParentPos.Y = 0;
                    currentBoneParentPos.Z = 0;
                }
                smdsw.WriteLine("  " + (i + 1) + " " + (currentBone.pivot.X - currentBoneParentPos.X) + " " + (currentBone.pivot.Z - currentBoneParentPos.Z) + " " + -(currentBone.pivot.Y - currentBoneParentPos.Y) + " " + "0.000000" + " " + "-0.000000" + " " + "0.000000");
            }
            smdsw.WriteLine("end");
            //Skeleton block end
            //Triangles block start
            smdsw.WriteLine("triangles");
            for (var i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                var currentTri = reader.model.skins[0].triangles[i];
                var vert1 = reader.model.vertices[currentTri.pt1];
                var vert2 = reader.model.vertices[currentTri.pt2];
                var vert3 = reader.model.vertices[currentTri.pt3];
                smdsw.WriteLine(Path.GetFileNameWithoutExtension(reader.model.textures[0].filename));
                string vert1line = "  " + (vert1.boneIndices_0 + 1) + " " + vert1.position.X + " " + vert1.position.Z + " " + -vert1.position.Y + " " + vert1.normal.X + " " + vert1.normal.Y + " " + vert1.normal.Z + " " + vert1.textureCoordX + " " + (vert1.textureCoordY - 1) * -1;
                string vert2line = "  " + (vert2.boneIndices_0 + 1) + " " + vert2.position.X + " " + vert2.position.Z + " " + -vert2.position.Y + " " + vert2.normal.X + " " + vert2.normal.Y + " " + vert2.normal.Z + " " + vert2.textureCoordX + " " + (vert2.textureCoordY - 1) * -1;
                string vert3line = "  " + (vert3.boneIndices_0 + 1) + " " + vert3.position.X + " " + vert3.position.Z + " " + -vert3.position.Y + " " + vert3.normal.X + " " + vert3.normal.Y + " " + vert3.normal.Z + " " + vert3.textureCoordX + " " + (vert3.textureCoordY - 1) * -1;
                int linksVert1 = 1;
                int linksVert2 = 1;
                int linksVert3 = 1;
                string addlink1;
                string addlink2;
                string addlink3;
                addlink1 = (vert1.boneIndices_0 + 1) + " " + vert1.boneWeight_0 / 255f;
                addlink2 = (vert2.boneIndices_0 + 1) + " " + vert2.boneWeight_0 / 255f;
                addlink3 = (vert3.boneIndices_0 + 1) + " " + vert3.boneWeight_0 / 255f;
                //-------------------------------------
                if (vert1.boneWeight_1 > 0)
                {
                    addlink1 = addlink1 + " " + (vert1.boneIndices_1 + 1) + " " + vert1.boneWeight_1 / 255f;
                    linksVert1++;
                }
                if (vert1.boneWeight_2 > 0)
                {
                    addlink1 = addlink1 + " " + (vert1.boneIndices_2 + 1) + " " + vert1.boneWeight_2 / 255f;
                    linksVert1++;
                }
                if (vert1.boneWeight_3 > 0)
                {
                    addlink1 = addlink1 + " " + (vert1.boneIndices_3 + 1) + " " + vert1.boneWeight_3 / 255f;
                    linksVert1++;
                }
                //-------------------------------------
                if (vert2.boneWeight_1 > 0)
                {
                    addlink2 = addlink2 + " " + (vert2.boneIndices_1 + 1) + " " + vert2.boneWeight_1 / 255f;
                    linksVert2++;
                }
                if (vert2.boneWeight_2 > 0)
                {
                    addlink2 = addlink2 + " " + (vert2.boneIndices_2 + 1) + " " + vert2.boneWeight_2 / 255f;
                    linksVert2++;
                }
                if (vert2.boneWeight_3 > 0)
                {
                    addlink2 = addlink2 + " " + (vert2.boneIndices_3 + 1) + " " + vert2.boneWeight_3 / 255f;
                    linksVert2++;
                }
                //-------------------------------------
                if (vert3.boneWeight_1 > 0)
                {
                    addlink3 = addlink3 + " " + (vert3.boneIndices_1 + 1) + " " + vert3.boneWeight_1 / 255f;
                    linksVert3++;
                }
                if (vert3.boneWeight_2 > 0)
                {
                    addlink3 = addlink3 + " " + (vert3.boneIndices_2 + 1) + " " + vert3.boneWeight_2 / 255f;
                    linksVert3++;
                }
                if (vert3.boneWeight_3 > 0)
                {
                    addlink3 = addlink3 + " " + (vert3.boneIndices_3 + 1) + " " + vert3.boneWeight_3 / 255f;
                    linksVert3++;
                }
                //-------------------------------------
                vert1line = vert1line + " " + linksVert1 + " " + addlink1;
                vert2line = vert2line + " " + linksVert2 + " " + addlink2;
                vert3line = vert3line + " " + linksVert3 + " " + addlink3;
                smdsw.WriteLine(vert1line);
                smdsw.WriteLine(vert2line);
                smdsw.WriteLine(vert3line);
            }
            smdsw.WriteLine("end");
            //Triangles block end
            smdsw.Close();
        }
        // https://developer.valvesoftware.com/wiki/Studiomdl_Data
    }
}
