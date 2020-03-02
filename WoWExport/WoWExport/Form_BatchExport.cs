using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace WoWExport
{
    public partial class Form_BatchExport : Form
    {
        public List<string> fileList;
        public static string[] AlphaType =
        {
            "RGB",
            "RGB (63)",
            "Alpha",
            "Alpha (63)"
        };

        public Form_BatchExport(List<string> receivedFileList)
        {
            InitializeComponent();
            fileList = receivedFileList;
        }

        private void Form_BatchExport_Load(object sender, EventArgs e)
        {
            groupBox1.Text = "Export Settings";
            comboBox1.Items.AddRange(AlphaType);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            checkBox1.Checked = Managers.ConfigurationManager.ADTExportM2;
            checkBox1.Text = "Export doodads (ADT)";
            checkBox2.Checked = Managers.ConfigurationManager.ADTExportWMO;
            checkBox2.Text = "Export world models (ADT)";
            checkBox3.Checked = Managers.ConfigurationManager.WMOExportM2;
            checkBox3.Text = "Export WMO doodads (WMO & ADT)";
            if (checkBox2.Checked)
            {
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
            }
            checkBox4.Checked = Managers.ConfigurationManager.ADTExportFoliage;
            checkBox4.Text = "Export foliage (ADT)";
            checkBox4.Enabled = false; //Disabled for the time being
            checkBox5.Checked = Managers.ConfigurationManager.ADTexportTextures;
            checkBox5.Text = "Export ground textures (ADT)";
            checkBox6.Checked = Managers.ConfigurationManager.ADTexportAlphaMaps;
            checkBox6.Text = "Export alphamaps (ADT)";

            comboBox1.SelectedIndex = Managers.ConfigurationManager.ADTAlphaMode;

            checkBox7.Checked = Managers.ConfigurationManager.ADTIgnoreHoles;
            checkBox7.Text = "Ignore holes (ADT)";

            checkBox8.Text = "Use transparency";
            checkBox8.Checked = Managers.ConfigurationManager.ADTAlphaUseA;

            for (int i = 0; i < fileList.Count; i++)
            {
                listBox1.Items.Add(Path.GetFileName(fileList[i]));
            }
            

            button1.Text = "Export";
            label1.Text = "Save to: " + Managers.ConfigurationManager.OutputDirectory;
            this.Text = "Batch export";
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

            Managers.ConfigurationManager.ADTAlphaMode = comboBox1.SelectedIndex;
            Managers.ConfigurationManager.ADTAlphaUseA = checkBox8.Checked;
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

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConfiguration();

            if (Managers.ConfigurationManager.OutputDirectory != null)
            {
                BatchExport();
                MessageBox.Show("Done");
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
        }

        private void BatchExport()
        {
            try
            {
                for (int i = 0; i < fileList.Count; i++)
                {
                    //Console.WriteLine("Exporting: " + fileList[i]);
                    string currentFile = fileList[i];
                    switch (Path.GetExtension(currentFile))
                    {
                        case ".adt":
                            Exporters.OBJ.ADTExporter.exportADT(currentFile, Managers.ConfigurationManager.OutputDirectory, "low"/* hardcoded for the moment */);
                            break;
                        case ".m2":
                            Exporters.OBJ.M2Exporter.ExportM2(currentFile, Managers.ConfigurationManager.OutputDirectory);
                            break;
                        case ".wmo":
                            Exporters.OBJ.WMOExporter.ExportWMO(currentFile, Managers.ConfigurationManager.OutputDirectory);
                            break;
                        case ".blp":
                            Exporters.BLPExporter.ExportBLP(currentFile, Managers.ConfigurationManager.OutputDirectory);
                            break;
                        default:
                            //some other files that the exporter does not care about
                            break;
                    }
                    //Console.WriteLine("Finished: " + fileList[i]);
                }
            }
            catch
            {

            }
        }
    }
}
