using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WoWFormatLib.FileReaders;

namespace WoWExport
{
    public partial class Form_M2Export : Form
    {
        private readonly BackgroundWorker exportworker = new BackgroundWorker();
        public String filename;

        string[] formats = {
            "OBJ",
            "SMD (experimental)"
        };

        public Form_M2Export(string receivedFilename)
        {
            InitializeComponent();
            filename = receivedFilename;

            exportworker.DoWork += exportworker_DoWork;
            exportworker.RunWorkerCompleted += exportworker_RunWorkerCompleted;
            exportworker.ProgressChanged += exportworker_ProgressChanged;
            exportworker.WorkerReportsProgress = true;
        }

        private void Form_M2Export_Load(object sender, EventArgs e)
        {
            button1.Text = "Export";
            this.Text = filename;
            label10.Text = "Export format:";
            comboBox1.Items.AddRange(formats);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = 0;
            label11.Hide();

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
                label9.Text = "Submeshes:" + reader.model.skins[0].submeshes.Count();
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
                button1.Enabled = false;
                label11.Show();
                exportworker.RunWorkerAsync(comboBox1.SelectedIndex);
            }
            else
            {
                throw new Exception("No output direcotry set");
            }
        }

        private void exportworker_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (e.Argument)
            {
                case 0: //obj
                    Exporters.OBJ.M2Exporter.ExportM2(filename, Managers.ConfigurationManager.OutputDirectory, exportworker);
                    break;
                case 1: //smd
                    Exporters.SMD.M2SmdExporter.ExportM2(filename, Managers.ConfigurationManager.OutputDirectory);
                    break;
                default:
                    break;
            }
        }

        private void exportworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            label11.Hide();
            progressBar1.Value = 100;
            MessageBox.Show("Done");
        }

        private void exportworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                label11.Text = state;
            }
            progressBar1.Value = e.ProgressPercentage;
        }

    }
}
