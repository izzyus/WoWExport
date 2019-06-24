﻿//-----------------------------------------------------------------------------------------------------------------
// This form will be debug only
//-----------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using WoWFormatLib.FileReaders;
using Generators.ADT_Alpha;
//Geometry
using Exporters.OBJ;
using StormLibSharp;

namespace WoWExport
{
    public partial class Form1 : Form
    {

        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------

        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();
        public List<String> GroundTextures = new List<String>();
        public string filePath = string.Empty;

        public bool exportLayersCSV = false;

        //-----------------------------------------------------------------------------------------------------------------


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Set picturebox defaults
            panel1.AutoScroll = true;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            //pictureBox1.Image = Image.FromFile("D:\\Xport\\WorldOfWarcraft\\ModelExport\\azeroth_32_48\\azeroth_32_48.jpg");

            this.Text = "WoW Export";

            button1.Enabled = false; //disabled indefinitely
            button1.Text = "Load";

            button2.Enabled = false;
            button2.Text = "Export";

            textBox1.Text = "D:\\export";

            groupBox1.Text = "Alphamap [#]";

            radioButton1.Checked = true;
            radioButton1.Text = "Uniform Grayscale";
            radioButton2.Text = "Uniform ARGB";
            radioButton3.Text = "Non-Uniform";

            //To be deleted later
            button3.Text = "Crash Me!";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            openFileDialog1.InitialDirectory = "C:\\";
            
            openFileDialog1.Title = "Open";
            openFileDialog1.DefaultExt = "adt";
            openFileDialog1.Filter = "WoW Terrain file (*.adt)|*.adt|All files (*.*)|*.*";

            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog1.FileName;

                if(filePath.Contains("_obj") || filePath.Contains("_tex") || filePath.Contains("_lod"))
                {
                    MessageBox.Show("You selected the wrong thing, dummy");
                    return;
                }

                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///FILE OPERATIONS
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------
                
                var ADTfile = filePath;
                var WDTfile = filePath.Substring(0, filePath.Length - 10) + ".wdt";
                var ADTobj = filePath.Replace(".adt", "_obj0.adt");
                var ADTtex = filePath.Replace(".adt", "_tex0.adt");


                //Clear listbox first
                listBox1.Items.Clear();
                //Clear arrays
                AlphaLayers.Clear();
                AlphaLayersNames.Clear();
                //Clear the picturebox
                pictureBox1.Image = null;
                //Reset groupbox name
                groupBox1.Text = "Alphamap [#]";



                //-----------------------------------------------------------------------------------------------------------------
                //Establish the generation mode:
                //-----------------------------------------------------------------------------------------------------------------
                int GenerationMode = 1;
                if (radioButton1.Checked)
                {
                    GenerationMode = 1;
                }
                else
                {
                    if (radioButton2.Checked)
                    {
                        GenerationMode = 2;
                    }
                    else
                    {
                        GenerationMode = 3;
                    }
                }


                //-----------------------------------------------------------------------------------------------------------------
                //File operations:
                //-----------------------------------------------------------------------------------------------------------------
                if (File.Exists(ADTfile) && File.Exists(WDTfile) && File.Exists(ADTobj) && File.Exists(ADTtex))
                {
                    //Read the ADT file:
                    ADTReader reader = new ADTReader();
                    //reader.LoadADT(ADTfile, WDTfile, ADTobj, ADTtex);

                    //Add in the listbox all the textures (+path) used by the adt file:
                    listBox1.Items.AddRange(reader.adtfile.textures.filenames);
                    GroundTextures = reader.adtfile.textures.filenames.ToList();

                    //Generate the alphamaps:
                    ADT_Alpha AlphaMapsGenerator = new ADT_Alpha();
                    //AlphaMapsGenerator.GenerateAlphaMaps(reader.adtfile);
                    AlphaMapsGenerator.GenerateAlphaMaps(reader.adtfile, GenerationMode);

                    //Assign layers and names
                    AlphaLayers = AlphaMapsGenerator.AlphaLayers;
                    AlphaLayersNames = AlphaMapsGenerator.AlphaLayersNames;

                    //Enable the export button if the generation was successful
                    if (AlphaLayers != null)
                    {
                        button2.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("One or more files are missing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button2.Enabled = false;
                }

                //Disable Layers CSV export button unless method 3 is used
                if (radioButton3.Checked)
                {
                    //button3.Enabled = true;
                    exportLayersCSV = true;
                }
                else
                {
                    //button3.Enabled = false;
                    exportLayersCSV = false;
                }

            



            //----------------------------------------------------------------------------------------------------------
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///FILE OPERATIONS END
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //----------------------------------------------------------------------------------------------------------
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Try to update the preview:
            if (!radioButton3.Checked)
            {
                try
                {
                    pictureBox1.Image = AlphaLayers[listBox1.SelectedIndex];
                    groupBox1.Text = "Alphamap [" + listBox1.SelectedIndex.ToString() + "]";
                }
                catch
                {
                    //Some error occured   
                }
            }
            else
            {
                groupBox1.Text = "Alphamap [PREVIEW DISABLED IN THIS MODE]";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            {
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///Export Button
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------

                //----------------------------------------------------------------------------------------------------------
                //Get the mapname
                //----------------------------------------------------------------------------------------------------------
                string mapname = filePath;
                mapname = mapname.Substring(mapname.LastIndexOf("\\", mapname.Length - 2) + 1);
                mapname = mapname.Substring(0, mapname.Length - 4);

                //----------------------------------------------------------------------------------------------------------
                //Create a folder with the map name (if non-existent) to save all everything in
                //----------------------------------------------------------------------------------------------------------
                if (!Directory.Exists(textBox1.Text + "\\" + mapname))
                {
                    try
                    {
                        Directory.CreateDirectory(textBox1.Text + "\\" + mapname + "\\");
                    }
                    catch
                    {
                        MessageBox.Show("Could not create folder: " + textBox1.Text + "\\" + mapname, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }


                //----------------------------------------------------------------------------------------------------------
                //Save ground textures
                //----------------------------------------------------------------------------------------------------------

                if (!Directory.Exists(textBox1.Text + "\\" + mapname + "\\textures\\"))
                {
                    try
                    {
                        Directory.CreateDirectory(textBox1.Text + "\\" + mapname + "\\textures\\");
                    }
                    catch
                    {
                        MessageBox.Show("Could not create folder: " + textBox1.Text + "\\" + mapname + "\\textures\\");
                    }

                }

                var blpreader = new BLPReader();
                foreach (string texture in GroundTextures)
                {
                    //MessageBox.Show("texture: " + texture);

                        if (!File.Exists(textBox1.Text + "\\" + mapname + "\\textures\\" + texture.Substring(texture.LastIndexOf("\\", texture.Length - 2) + 1).Replace("blp", "png")))
                        {
                            if(File.Exists(@"D:\mpqediten32\Work\" + texture))
                            {
                                try
                                {
                                    //blpreader.LoadBLP(@"D:\mpqediten32\Work\" + texture); //TO BE FIXED <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                                    blpreader.bmp.Save(textBox1.Text + "\\" + mapname + "\\textures\\" + texture.Substring(texture.LastIndexOf("\\", texture.Length - 2) + 1).Replace("blp", "png"));
                                }
                                catch
                                {
                                    MessageBox.Show("Could not save file: " + textBox1.Text + "\\" + mapname + "\\textures\\" + texture.Substring(texture.LastIndexOf("\\", texture.Length - 2) + 1).Replace("blp", "png"));
                                }
                            }
                            else
                            {
                                MessageBox.Show(@"Missing file: D:\mpqediten32\Work\" + texture);
                            }
                        }
                }



                //----------------------------------------------------------------------------------------------------------
                //Create a folder for the alphamaps (if none-xistent) to save the alphas separately
                //----------------------------------------------------------------------------------------------------------
                if (!Directory.Exists(textBox1.Text + "\\" + mapname + "\\alphamaps\\"))
                {
                    try
                    {
                        Directory.CreateDirectory(textBox1.Text + "\\" + mapname + "\\alphamaps\\");
                    }
                    catch
                    {
                        MessageBox.Show("Could not create folder: " + textBox1.Text + "\\" + mapname + "\\alphamaps\\");
                    }

                }

                //----------------------------------------------------------------------------------------------------------
                //Save alpha maps
                //----------------------------------------------------------------------------------------------------------
                for (int m = 0; m < AlphaLayers.ToArray().Length; m++)
                {
                    try
                    {
                        //AlphaLayers[m].Save(textBox2.Text + "\\" + mapname + "-" + AlphaLayersNames[m] + ".png");
                        //AlphaLayers[m].Save(textBox2.Text + "\\" + mapname + "\\" + mapname + "-" + AlphaLayersNames[m] + ".png");
                        //AlphaLayers[m].Save(textBox2.Text + "\\" + mapname + "\\" + mapname + "-" + AlphaLayersNames[m].Replace(";", "_") + ".png");
                        AlphaLayers[m].Save(textBox1.Text + "\\" + mapname + "\\alphamaps\\" + mapname + "-" + AlphaLayersNames[m].Replace(";", "_") + ".png");
                    }
                    catch
                    {
                        MessageBox.Show("Could not export the alpha maps", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                //----------------------------------------------------------------------------------------------------------
                //Export Layers CSV info
                //----------------------------------------------------------------------------------------------------------
                if (exportLayersCSV)
                {

                    if (File.Exists(textBox1.Text + "\\" + mapname + "\\" + mapname + "_" + "layers.csv"))
                    {
                        File.Delete(textBox1.Text + "\\" + mapname + "\\" + mapname + "_" + "layers.csv");
                    }

                    string LineOfText = "";
                    int cchunk = 0;
                    for (int i = 0; i < AlphaLayersNames.ToArray().Length; i++)
                    {
                        var line = AlphaLayersNames[i];
                        var values = line.Split(';');
                        var chunk = int.Parse(values[0]);
                        if (chunk == cchunk)
                        {
                            LineOfText = LineOfText + values[0] + ";" + values[1] + ";" + values[2] + ";";
                        }
                        else //Next Chunk
                        {
                            //File.AppendAllText(textBox2.Text + "\\" + mapname + "_" + "layers.csv", LineOfText.Substring(0, LineOfText.Length - 1) + Environment.NewLine);
                            File.AppendAllText(textBox1.Text + "\\" + mapname + "\\" + mapname + "_" + "layers.csv", LineOfText.Substring(0, LineOfText.Length - 1) + Environment.NewLine);
                            LineOfText = values[0] + ";" + values[1] + ";" + values[2] + ";";
                            cchunk++;
                        }
                    }
                    //Last entry, i have no idea how to do it properly so i am doing it like this
                    //File.AppendAllText(textBox2.Text + "\\" + mapname + "_" + "layers.csv", LineOfText.Substring(0, LineOfText.Length - 1));
                    File.AppendAllText(textBox1.Text + "\\" + mapname + "\\" + mapname + "_" + "layers.csv", LineOfText.Substring(0, LineOfText.Length - 1));
                }
                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///Export Button End
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------

                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///Geometry - experimental
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------

                ADTExporter Xporter = new ADTExporter();
                //Xporter.exportADT(filePath, textBox1.Text + "\\", "low");

                //----------------------------------------------------------------------------------------------------------
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ///Geometry - end
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //----------------------------------------------------------------------------------------------------------
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //ADTReader335 reader = new ADTReader335();
            //reader.LoadADT(ADTfile, WDTfile);
            //reader.LoadADT("D:\\mpqediten32\\335\\Work\\World\\Maps\\Azeroth\\Azeroth_31_60.adt", "D:\\mpqediten32\\335\\Work\\World\\Maps\\Azeroth\\Azeroth.wdt");

            //MessageBox.Show("V:"+reader.adtfile.version); //debug
            //listBox1.Items.AddRange(reader.m2Files.ToArray()); //debug
            //listBox1.Items.AddRange(reader.wmoFiles.ToArray()); //debug
            //WMOExporter Xporter = new WMOExporter();
            //Xporter.ExportWMO(@"D:\mpqediten32\Work\World\wmo\Azeroth\Buildings\human_farm\farm_closed.wmo", textBox1.Text + "//");
            /*
            M2Reader Reader = new M2Reader();
            Reader.LoadM2(@"D:\test\335\World\AZEROTH\ELWYNN\PASSIVEDOODADS\ELWYNNFENCES\ElwynnWoodPost01.m2");
            MessageBox.Show("Version: " + Reader.model.version + Environment.NewLine +
                            "Name: " + Reader.model.name + Environment.NewLine +
                            "Sequences: " + Reader.model.sequences.Count() + Environment.NewLine +
                            "Animations: " + Reader.model.animations.Count() + Environment.NewLine +
                            "Bones: " + Reader.model.bones.Count() + Environment.NewLine +
                            "Vertices: " + Reader.model.vertices.Count() + Environment.NewLine +
                            "Skins: " + Reader.model.skins.Count() + Environment.NewLine +
                            "Textures: " + Reader.model.textures.Count(), "Info");
            for (int i = 0; i < Reader.model.textures.Count(); i++)
            {
                MessageBox.Show("Texture " + i + ": " + Reader.model.textures[i].filename);
            }
            for (int i = 0; i < Reader.model.skins.Count(); i++)
            {
                MessageBox.Show("Skin " + i + ": " + Reader.model.skins[i].filename);
            }
            M2Exporter Xporter = new M2Exporter();
            Xporter.ExportM2(@"D:\test\335\World\AZEROTH\ELWYNN\PASSIVEDOODADS\ELWYNNFENCES\ElwynnWoodPost01.m2", textBox1.Text + "\\");
            */
            //MpqArchive CurrentArchive = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\art.mpq", FileAccess.Read);

            //WoWExport.Managers.ArchiveManager ArchiveMan = new WoWExport.Managers.ArchiveManager();

            Managers.ArchiveManager.GameDir = @"D:\World of Warcraft - Cataclysm";
            //Managers.ArchiveManager.GameDir = @"D:\World of Warcraft - Wrath of the Lich King";
            //Managers.ArchiveManager.GenerateMainListFile();

            Managers.ArchiveManager.LoadArchives();

            Managers.ArchiveManager.GenerateMainListFileFromMPQ();
            //Managers.ArchiveManager.ExtractListfiles(textBox1.Text + "\\");

            //BLPReader reader = new BLPReader();
            //reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(@"TEST\TOTALLYRAD.BLP"));
            //reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(textBox1.Text));
            //pictureBox1.Image = reader.bmp;

            //MessageBox.Show("Exists: " + Managers.ArchiveManager.FileExists(textBox1.Text));
            //World\maps\Azeroth\Azeroth_31_45.adt

            //ADTExporter Xporter = new ADTExporter();
            //Xporter.exportADT(@"World\maps\Azeroth\Azeroth_32_48.adt", textBox1.Text + "\\", "low");

            //M2Exporter Xporter = new M2Exporter();
            //Xporter.ExportM2(@"World\AZEROTH\ELWYNN\PASSIVEDOODADS\ELWYNNFENCES\ElwynnWoodPost01.m2", textBox1.Text + "\\");

            //WMOExporter.ExportWMO(@"World\wmo\Azeroth\Buildings\human_farm\farm_closed.wmo", textBox1.Text + "\\");
            //ADTExporter.exportADT(@"World\maps\Azeroth\azeroth_28_52.adt", textBox1.Text + "//","low");

            Managers.ConfigurationManager.ADTExportM2 = true;
            Managers.ConfigurationManager.ADTExportWMO = true;
            //Managers.ConfigurationManager.ADTExportFoliage = false; //Obsolete atm
            Managers.ConfigurationManager.ADTexportTextures = true;
            Managers.ConfigurationManager.ADTexportAlphaMaps = true;
            Managers.ConfigurationManager.WMOExportM2 = true;

            ADTExporter.exportADT(@"World\maps\Azeroth\azeroth_31_49.adt", textBox1.Text + "//", "low");

            /*
            ADTReader reader = new ADTReader();
            reader.LoadADT(@"World\maps\Azeroth\azeroth_31_49.adt");
            for (var mi = 0; mi < reader.adtfile.objects.worldModels.entries.Count(); mi++)
            {
                var wmo = reader.adtfile.objects.worldModels.entries[mi];
                MessageBox.Show(" X:" + wmo.position.X + " Y:" + wmo.position.Y + " Z:" + wmo.position.Z + " RX:" + wmo.rotation.X + " RY:" + wmo.rotation.Y + " RZ:" + wmo.rotation.Z + " S:" + wmo.scale / 1024f + " NSet:" + wmo.nameSet);
            }
            */

            //M2Exporter.ExportM2(@"WORLD\GENERIC\PASSIVEDOODADS\PARTICLEEMITTERS\HOUSESMOKE.M2", textBox1.Text + "//");
            /*
            M2Reader Reader = new M2Reader();
            Reader.LoadM2(@"WORLD\GENERIC\PASSIVEDOODADS\PARTICLEEMITTERS\HOUSESMOKE.M2");
            MessageBox.Show("Version: " + Reader.model.version + Environment.NewLine +
                            "Name: " + Reader.model.name + Environment.NewLine +
                            "Sequences: " + Reader.model.sequences.Count() + Environment.NewLine +
                            "Animations: " + Reader.model.animations.Count() + Environment.NewLine +
                            "Bones: " + Reader.model.bones.Count() + Environment.NewLine +
                            "Vertices: " + Reader.model.vertices.Count() + Environment.NewLine +
                            "Skins: " + Reader.model.skins.Count() + Environment.NewLine +
                            "Textures: " + Reader.model.textures.Count(), "Info");
            for (int i = 0; i < Reader.model.textures.Count(); i++)
            {
                MessageBox.Show("Texture " + i + ": " + Reader.model.textures[i].filename);
            }
            for (int i = 0; i < Reader.model.skins.Count(); i++)
            {
                MessageBox.Show("Skin " + i + ": " + Reader.model.skins[i].filename);
            }
            */

        }
    }
}
