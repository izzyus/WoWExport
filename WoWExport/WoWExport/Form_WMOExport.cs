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

            checkBox2.Text = "Doodads use global paths";
            checkBox2.Checked = Managers.ConfigurationManager.WMODoodadsGlobalPath;
            checkBox3.Text = "Doodads placement global paths";
            checkBox3.Checked = Managers.ConfigurationManager.WMODoodadsPlacementGlobalPath;

            button1.Text = "Export";
            this.Text = filename;

            try
            {
                WMOReader reader = new WMOReader();
                reader.LoadWMO(filename);

                if (reader.wmofile.doodadNames != null)
                {
                    try
                    {
                        for(int i = 0; i < reader.wmofile.doodadNames.Length; i ++)
                        {
                            listBox1.Items.Add(reader.wmofile.doodadNames[i].filename.ToLower());
                        }
                    }
                    catch
                    {

                    }
                }

                //other stuff here
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private void UpdateConfiguration()
        {
            Managers.ConfigurationManager.WMOExportM2 = checkBox1.Checked;
            Managers.ConfigurationManager.WMODoodadsGlobalPath = checkBox2.Checked ;
            Managers.ConfigurationManager.WMODoodadsPlacementGlobalPath = checkBox3.Checked;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Enabled = true;
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
            }
        }
    }
}
