﻿using System;
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

                //Populate model details:

                label1.Text = "Version: " + reader.model.version;
                label2.Text = "Name: " + reader.model.name;
                label3.Text = "Sequences: " + reader.model.sequences.Count();
                label4.Text = "Animations: " + reader.model.animations.Count();
                label5.Text = "Bones: " + reader.model.bones.Count();
                label6.Text = "Vertices: " + reader.model.vertices.Count();
                label7.Text = "Skins: " + reader.model.skins.Count();
                label8.Text = "Textures: " + reader.model.textures.Count();         
            }
            catch (Exception ex)
            {
                throw new Exception("Could not read the model to display details" + " - exception: " + ex.Message);
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
