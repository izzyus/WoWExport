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


namespace WoWExport
{
    public partial class Form1 : Form
    {

        //-----------------------------------------------------------------------------------------------------------------
        //PUBLIC STUFF:
        //-----------------------------------------------------------------------------------------------------------------

        public List<Bitmap> AlphaLayers = new List<Bitmap>();
        public List<String> AlphaLayersNames = new List<String>();
        public string filePath = string.Empty;


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


            button1.Text = "Load";

            this.Text = "WoW Export";

            groupBox1.Text = "Alphamap [#]";

            radioButton1.Checked = true;
            radioButton1.Text = "Uniform Grayscale";
            radioButton2.Text = "Uniform ARGB";
            radioButton3.Text = "Non-Uniform";


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
                    reader.LoadADT(ADTfile, WDTfile, ADTobj, ADTtex);

                    //Add in the listbox all the textures (+path) used by the adt file:
                    listBox1.Items.AddRange(reader.adtfile.textures.filenames);

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
                        //button2.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("One or more files are missing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //button2.Enabled = false;
                }

                //Disable Layers CSV export button unless method 3 is used
                if (radioButton3.Checked)
                {
                    //button3.Enabled = true;
                }
                else
                {
                    //button3.Enabled = false;
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
    }
}
