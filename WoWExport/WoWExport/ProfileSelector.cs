using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WoWExport
{
    public partial class ProfileSelector : Form
    {

        public List<string> Profiles;

        public static string[] DisplayProfiles =
        {
            "Vanilla",
            "TBC",
            "LK",
            "Cata",
            "MOP",
            "WOD",
            "Legion",
            "BFA"
        };

        public ProfileSelector()
        {
            InitializeComponent();
        }

        private void ProfileSelector_Load(object sender, EventArgs e)
        {
            button1.Text = "Load";
            button2.Text = "Browse";

            textBox1.Enabled = false;

            comboBox1.Items.AddRange(DisplayProfiles);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = 0;

        }

        private void BrowseFolder()
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BrowseFolder();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwitchProfile();
        }

        private void LoadProfiles()
        {

            using (StreamReader sr = File.OpenText(Environment.CurrentDirectory + "\\" + "profiles.txt"))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    //MainListFile.Add(s.ToLower() + ";" + fileInfo.Name.Replace(".txt", ".mpq"));
                    Profiles.Add(s);
                }
            }

        }

        private void SwitchProfile()
        {



        }



        private void button1_Click_1(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && comboBox1.Text != "")
            {
                Managers.ConfigurationManager.Profile = comboBox1.Text;
                Managers.ConfigurationManager.GameDir = textBox1.Text;
                new WoWExport.Form1().Show();
                this.Hide();
            }
            else
            {
                if(textBox1.Text == "")
                {
                    BrowseFolder();
                }
            }
        }
    }
}
