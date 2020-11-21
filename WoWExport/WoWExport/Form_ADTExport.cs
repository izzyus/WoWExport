using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

using WoWFormatLib.FileReaders;

namespace WoWExport
{
    public partial class Form_ADTExport : Form
    {
        private readonly BackgroundWorker exportworker = new BackgroundWorker();
        public String filename;

        public static string[] AlphaType =
        {
            "RGB",
            "RGB (63)",
            "Alpha",
            "Alpha (63)",
            "Splatmaps"
        };

        public Form_ADTExport(string receivedFilename)
        {
            InitializeComponent();
            filename = receivedFilename;

            exportworker.DoWork += exportworker_DoWork;
            exportworker.RunWorkerCompleted += exportworker_RunWorkerCompleted;
            exportworker.ProgressChanged += exportworker_ProgressChanged;
            exportworker.WorkerReportsProgress = true;
        }

        private void Form_ADTExport_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(AlphaType);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            label4.Hide();

            checkBox1.Checked = Managers.ConfigurationManager.ADTExportM2;
            checkBox1.Text = "Export doodads";
            checkBox2.Checked = Managers.ConfigurationManager.ADTExportWMO;
            checkBox2.Text = "Export world models";
            checkBox3.Checked = Managers.ConfigurationManager.WMOExportM2;
            checkBox3.Text = "Export WMO doodads";
            if (checkBox2.Checked)
            {
                checkBox3.Enabled = true;
                checkBox10.Enabled = true;
                checkBox11.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
                checkBox10.Enabled = false;
                checkBox11.Enabled = false;
            }
            checkBox4.Checked = Managers.ConfigurationManager.ADTExportFoliage;
            checkBox4.Text = "Export foliage";
            checkBox4.Enabled = false; //Disabled for the time being
            checkBox5.Checked = Managers.ConfigurationManager.ADTexportTextures;
            checkBox5.Text = "Export ground textures";

            checkBox12.Checked = Managers.ConfigurationManager.ADTPreserveTextureStruct;
            checkBox12.Text = "Preserve textures path";
            if (checkBox5.Checked)
            {
                checkBox12.Enabled = true;
                checkBox13.Enabled = true;
            }
            else
            {
                checkBox12.Enabled = false;
                checkBox13.Enabled = false;
            }

            checkBox6.Checked = Managers.ConfigurationManager.ADTexportAlphaMaps;
            checkBox6.Text = "Export alphamaps";

            comboBox1.SelectedIndex = Managers.ConfigurationManager.ADTAlphaMode;

            checkBox7.Checked = Managers.ConfigurationManager.ADTIgnoreHoles;
            checkBox7.Text = "Ignore holes";

            checkBox8.Text = "Use transparency";
            checkBox8.Checked = Managers.ConfigurationManager.ADTAlphaUseA;

            checkBox9.Text = "ADT model placement global paths";
            checkBox9.Checked = Managers.ConfigurationManager.ADTModelsPlacementGlobalPath;
            checkBox10.Text = "WMO Doodads use global paths";
            checkBox10.Checked = Managers.ConfigurationManager.WMODoodadsGlobalPath;
            checkBox11.Text = "WMO Doodads placement global paths";
            checkBox11.Checked = Managers.ConfigurationManager.WMODoodadsPlacementGlobalPath;

            button1.Text = "Export";
            this.Text = filename;

            checkBox13.Text = "Export ground \"specular?\" textures";
            checkBox13.Checked = Managers.ConfigurationManager.ADTExportSpecularTextures;

            //-------------------------------------------------------------------------------------
            //Not really the intended use for this function, but for now it will stay like this.
            //-------------------------------------------------------------------------------------
            checkBox14.Text = "Split materials per chunk";
            if (Managers.ConfigurationManager.ADTQuality == "high")
            {
                checkBox14.Checked = true;
            }
            else
            {
                checkBox14.Checked = false;
            }
            //-------------------------------------------------------------------------------------

            if (checkBox14.Checked)
            {
                checkBox15.Enabled = true;
            }
            else
            {
                checkBox15.Enabled = false;
            }

            checkBox15.Text = "Split mesh per chunk";
            checkBox15.Checked = Managers.ConfigurationManager.ADTSplitChunks;

            checkBox16.Checked = Managers.ConfigurationManager.ADTexportAlphaMaps;
            checkBox16.Text = "Export heightmap";

            try
            {
                ADTReader reader = new ADTReader();
                if (Managers.ConfigurationManager.Profile <= 3) //WoTLK and below
                {
                    reader.Load335ADT(filename);
                }
                else
                {
                    reader.LoadADT(filename);
                }
                listBox1.Items.AddRange(reader.m2Files.Select(s => s.ToLowerInvariant()).ToArray());
                listBox2.Items.AddRange(reader.wmoFiles.Select(s => s.ToLowerInvariant()).ToArray());

                label1.Text = "Textures (" + reader.adtfile.textures.filenames.Count() + ")";
                label2.Text = "Doodads (" + reader.m2Files.Count() + ")";
                label3.Text = "WMOs (" + reader.wmoFiles.Count() + ")";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void UpdateConfiguration()
        {
            Managers.ConfigurationManager.ADTExportM2 = checkBox1.Checked;
            Managers.ConfigurationManager.ADTExportWMO = checkBox2.Checked;
            Managers.ConfigurationManager.WMOExportM2 = checkBox3.Checked;
            Managers.ConfigurationManager.ADTExportFoliage = checkBox4.Checked;
            Managers.ConfigurationManager.ADTexportTextures = checkBox5.Checked;
            Managers.ConfigurationManager.ADTexportAlphaMaps = checkBox6.Checked;
            Managers.ConfigurationManager.ADTIgnoreHoles = checkBox7.Checked;
            Managers.ConfigurationManager.ADTExportHeightmap = checkBox16.Checked;

            Managers.ConfigurationManager.ADTAlphaMode = comboBox1.SelectedIndex;
            Managers.ConfigurationManager.ADTAlphaUseA = checkBox8.Checked;

            Managers.ConfigurationManager.ADTModelsPlacementGlobalPath = checkBox9.Checked;
            Managers.ConfigurationManager.WMODoodadsGlobalPath = checkBox10.Checked;
            Managers.ConfigurationManager.WMODoodadsPlacementGlobalPath = checkBox11.Checked;

            Managers.ConfigurationManager.ADTPreserveTextureStruct = checkBox12.Checked;

            Managers.ConfigurationManager.ADTExportSpecularTextures = checkBox13.Checked;
            //----------------------------------------------------------------
            //Again, not the intended use for this...
            //----------------------------------------------------------------
            if (checkBox14.Checked)
            {
                Managers.ConfigurationManager.ADTQuality = "high";
            }
            else
            {
                Managers.ConfigurationManager.ADTQuality = "low";
            }
            //----------------------------------------------------------------
            Managers.ConfigurationManager.ADTSplitChunks = checkBox15.Checked;
        }
        private void exportworker_DoWork(object sender, DoWorkEventArgs e)
        {
            Exporters.OBJ.ADTExporter.exportADT(filename, Managers.ConfigurationManager.OutputDirectory, Managers.ConfigurationManager.ADTQuality, exportworker);
        }

        private void exportworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            label4.Hide();
            progressBar1.Value = 100;
            MessageBox.Show("Done");
        }

        private void exportworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                label4.Text = state;
            }
            progressBar1.Value = e.ProgressPercentage;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox3.Enabled = true;
                checkBox10.Enabled = true;
                checkBox11.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
                checkBox10.Enabled = false;
                checkBox11.Enabled = false;
            }

            if (checkBox2.Checked || checkBox1.Checked)
            {
                checkBox9.Enabled = true;
            }
            else
            {
                checkBox9.Enabled = false;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConfiguration();
            if (Managers.ConfigurationManager.OutputDirectory != null)
            {
                label4.Show();
                button1.Enabled = false;
                exportworker.RunWorkerAsync();
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                comboBox1.Enabled = true;
                if (comboBox1.SelectedIndex > 1)
                {
                    checkBox8.Enabled = true;
                }
            }
            else
            {
                comboBox1.Enabled = false;
                checkBox8.Enabled = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > 1)
            {
                checkBox8.Enabled = true;
            }
            else
            {
                checkBox8.Enabled = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked || checkBox2.Checked)
            {
                checkBox9.Enabled = true;
            }
            else
            {
                checkBox9.Enabled = false;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                checkBox12.Enabled = true;
                checkBox13.Enabled = true;
            }
            else
            {
                checkBox12.Enabled = false;
                checkBox13.Enabled = false;
            }
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox14.Checked)
            {
                checkBox15.Enabled = true;
            }
            else
            {
                checkBox15.Enabled = false;
            }
        }
    }
}
