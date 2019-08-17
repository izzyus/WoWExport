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
    public partial class Form_M2Export : Form
    {

        public String filename;

        public Form_M2Export(string receivedFilename)
        {
            InitializeComponent();
            filename = receivedFilename;
        }

        private void Form_M2Export_Load(object sender, EventArgs e)
        {
            button1.Text = "Export";
            this.Text = filename;

            try
            {
                M2Reader reader = new M2Reader();
                reader.LoadM2(filename);

                //other stuff here
            }
            catch
            {
                throw new Exception("Something really really bad happened");
            }
        }

        private void UpdateConfiguration()
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConfiguration();

            if (Managers.ConfigurationManager.OutputDirectory != null)
            {
                Exporters.OBJ.M2Exporter.ExportM2(filename, Managers.ConfigurationManager.OutputDirectory);
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
            MessageBox.Show("Done");
        }
    }
}
