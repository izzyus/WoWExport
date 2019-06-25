using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WoWFormatLib.FileReaders;

namespace WoWExport
{
    public partial class Form_ADTExport : Form
    {
        
        public String filename;

        public Form_ADTExport(string receivedFilename)
        {
            InitializeComponent();
            filename = receivedFilename;
        }

        private void Form_ADTExport_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Managers.ConfigurationManager.ADTExportM2;
            checkBox1.Text = "Export doodads";
            checkBox2.Checked = Managers.ConfigurationManager.ADTExportWMO;
            checkBox2.Text = "Export world models";
            checkBox3.Checked = Managers.ConfigurationManager.WMOExportM2;
            checkBox3.Text = "Export WMO doodads";
            if (checkBox2.Checked)
            {
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
            }
            checkBox4.Checked = Managers.ConfigurationManager.ADTExportFoliage;
            checkBox4.Text = "Export foliage";
            checkBox4.Enabled = false; //Disabled for the time being
            checkBox5.Checked = Managers.ConfigurationManager.ADTexportTextures;
            checkBox5.Text = "Export ground textures";
            checkBox6.Checked = Managers.ConfigurationManager.ADTexportAlphaMaps;
            checkBox6.Text = "Export alphamaps";

            button1.Text = "Export";
            this.Text = filename;

            try
            {
                ADTReader reader = new ADTReader();
                reader.LoadADT(filename);

                //listBox1.Items.AddRange(reader.m2Files.ToArray());
                //listBox2.Items.AddRange(reader.wmoFiles.ToArray());
                listBox1.Items.AddRange(reader.m2Files.Select(s => s.ToLowerInvariant()).ToArray());
                listBox2.Items.AddRange(reader.wmoFiles.Select(s => s.ToLowerInvariant()).ToArray());

                label1.Text = "Textures (" + reader.adtfile.textures.filenames.Count() + ")";
                label2.Text = "Doodads (" + reader.m2Files.Count() + ")";
                label3.Text = "WMOs (" + reader.wmoFiles.Count() + ")";

            }
            catch
            {
                throw new Exception("Something really really bad happened");
            }
        }

        private void UpdateConfiguration()
        {
            Managers.ConfigurationManager.ADTExportM2 = checkBox1.Checked;
            Managers.ConfigurationManager.ADTExportWMO = checkBox2.Checked;
            Managers.ConfigurationManager.WMOExportM2 = checkBox3.Checked;
            Managers.ConfigurationManager.ADTExportFoliage = checkBox4.Checked;
            Managers.ConfigurationManager.ADTexportTextures = checkBox5.Checked;
            Managers.ConfigurationManager.ADTexportAlphaMaps= checkBox6.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConfiguration();

            if (Managers.ConfigurationManager.OutputDirectory != null)
            {
                Exporters.OBJ.ADTExporter.exportADT(filename, Managers.ConfigurationManager.OutputDirectory, "low"/* hardcoded for the moment */);
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
        }
    }
}
