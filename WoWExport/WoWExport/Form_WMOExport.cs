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
    public partial class Form_WMOExport : Form
    {

        public String filename;

        public Form_WMOExport(string receivedFilename)
        {
            InitializeComponent();
            filename = receivedFilename;
        }

        private void Form_WMOExport_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Managers.ConfigurationManager.WMOExportM2;
            checkBox1.Text = "Export doodads";
            button1.Text = "Export";
            this.Text = filename;

            try
            {
                WMOReader reader = new WMOReader();
                reader.LoadWMO(filename);

                //other stuff here
            }
            catch
            {
                throw new Exception("Something really really bad happened");
            }

        }

        private void UpdateConfiguration()
        {
            Managers.ConfigurationManager.WMOExportM2 = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConfiguration();

            if (Managers.ConfigurationManager.OutputDirectory != null)
            {
                Exporters.OBJ.WMOExporter.ExportWMO(filename, Managers.ConfigurationManager.OutputDirectory);
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
            MessageBox.Show("Done");
        }
    }
}
