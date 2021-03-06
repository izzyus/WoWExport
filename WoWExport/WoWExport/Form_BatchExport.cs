﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.ComponentModel;

namespace WoWExport
{
    public partial class Form_BatchExport : Form
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public List<string> fileList;
        public static string[] AlphaType =
        {
            "RGB",
            "RGB (63)",
            "Alpha",
            "Alpha (63)",
            "Splatmaps"
        };

        public Form_BatchExport(List<string> receivedFileList)
        {
            InitializeComponent();
            fileList = receivedFileList;

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
        }

        private void Form_BatchExport_Load(object sender, EventArgs e)
        {
            label2.Hide();

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
            checkBox4.Text = "Export foliage (ADT)";
            checkBox5.Checked = Managers.ConfigurationManager.ADTexportTextures;
            checkBox5.Text = "Export ground textures (ADT)";
            checkBox6.Text = "Export alphamaps (ADT)";
            checkBox6.Checked = Managers.ConfigurationManager.ADTExportHeightmap;

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

            comboBox1.SelectedIndex = Managers.ConfigurationManager.ADTAlphaMode;

            checkBox7.Checked = Managers.ConfigurationManager.ADTIgnoreHoles;
            checkBox7.Text = "Ignore holes (ADT)";

            checkBox8.Text = "Use transparency";
            checkBox8.Checked = Managers.ConfigurationManager.ADTAlphaUseA;

            checkBox9.Text = "ADT model placement global paths";
            checkBox9.Checked = Managers.ConfigurationManager.ADTModelsPlacementGlobalPath;
            checkBox10.Text = "WMO Doodads use global paths";
            checkBox10.Checked = Managers.ConfigurationManager.WMODoodadsGlobalPath;
            checkBox11.Text = "WMO Doodads placement global paths";
            checkBox11.Checked = Managers.ConfigurationManager.WMODoodadsPlacementGlobalPath;

            listView1.View = View.List;
            for (int i = 0; i < fileList.Count; i++)
            {
                listView1.Items.Add(Path.GetFileName(fileList[i]));
            }


            button1.Text = "Export";
            label1.Text = "Exporting to: " + Managers.ConfigurationManager.OutputDirectory;
            this.Text = "Batch export";

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

            checkBox16.Checked = Managers.ConfigurationManager.ADTExportHeightmap;
            checkBox16.Text = "Export heightmap (ADT)";

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
            if (comboBox1.SelectedIndex > 1 && comboBox1.SelectedIndex != 4)
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
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
        }

        private void BatchExport()
        {
            button1.Enabled = false;
            label2.Show();
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string exportDataJson = "[";
            bool hasADT = false;

            for (int i = 0; i < fileList.Count; i++)
            {
                listViewChangeColor(i, Color.Yellow, Color.DarkGoldenrod);
                bool skipped = false;
                try
                {
                    //Console.WriteLine("Exporting: " + fileList[i]);
                    string currentFile = fileList[i];
                    switch (Path.GetExtension(currentFile))
                    {
                        case ".adt":
                            hasADT = true;
                            string exportLoc = Managers.ConfigurationManager.OutputDirectory;
                            string adtName = Path.GetFileNameWithoutExtension(currentFile);
                            string wdtName = adtName.Substring(0,adtName.IndexOf("_",0));
                            string adtCoordX = adtName.Substring(adtName.IndexOf("_",0)+1, 2);
                            string adtCoordY = adtName.Substring(adtName.IndexOf("_", adtName.IndexOf("_", 0) + 1) +1, 2); ;
                            string matJson = Path.Combine(exportLoc,adtName) + "_MaterialData.json";
                            string heightJson = Path.Combine(exportLoc, adtName) + "_HeightData.json";
                            string placementCSV = Path.Combine(exportLoc, adtName) + "_ModelPlacementInformation.csv";
                            exportDataJson += String.Format("[\"{0}\",{1},{2},\"{3}\",\"{4}\",\"{5}\"],", wdtName,adtCoordX,adtCoordY,placementCSV,heightJson,matJson);

                            Exporters.OBJ.ADTExporter.exportADT(currentFile, Managers.ConfigurationManager.OutputDirectory, Managers.ConfigurationManager.ADTQuality, worker);
                            break;
                        case ".m2":
                            Exporters.OBJ.M2Exporter.ExportM2(currentFile, Managers.ConfigurationManager.OutputDirectory, worker);
                            break;
                        case ".wmo":
                            Exporters.OBJ.WMOExporter.ExportWMO(currentFile, Managers.ConfigurationManager.OutputDirectory, worker);
                            break;
                        case ".blp":
                            Exporters.BLPExporter.ExportBLP(currentFile, Managers.ConfigurationManager.OutputDirectory);
                            break;
                        default:
                            //some other files that the exporter does not care about
                            skipped = true;
                            break;
                    }
                    //Console.WriteLine("Finished: " + fileList[i]);
                    if (!skipped)
                    {
                        listViewChangeColor(i, Color.LightGreen, Color.DarkGreen);
                    }
                    else
                    {
                        listViewChangeColor(i, Color.DimGray, Color.DarkGray);
                    }
                }
                catch
                {
                    //Error occured
                    listViewChangeColor(i, Color.Pink, Color.DarkRed);
                }
            }

            if (hasADT)
            {
                exportDataJson = exportDataJson.Substring(0, exportDataJson.Length - 1); // Remove tailing comma
                exportDataJson += "]";
                exportDataJson = exportDataJson.Replace("\\","\\\\");
                File.WriteAllText(Managers.ConfigurationManager.OutputDirectory + "Experimental_ExportData.json", exportDataJson);
            }

        }
        private void listViewChangeColor(int index, Color back, Color fore) //horrible solution, but it works
        {
            listView1.Invoke(new MethodInvoker(delegate
            {
                listView1.Items[index].BackColor = back;
                listView1.Items[index].ForeColor = fore;
            }));
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            progressBar1.Value = 100;
            label2.Hide();
            MessageBox.Show("Done");
        }


        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                label2.Text = state;
            }
            progressBar1.Value = e.ProgressPercentage;
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

