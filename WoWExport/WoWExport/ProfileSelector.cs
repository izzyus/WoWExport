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
            label1.Text = "";

            button1.Text = "Load";
            button2.Text = "Browse";

            textBox1.Enabled = false;

            comboBox1.Items.AddRange(DisplayProfiles);
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            if (File.Exists(Environment.CurrentDirectory + "\\settings\\" + "last_session" + ".txt"))
            {
                try
                {
                    comboBox1.SelectedIndex = int.Parse(File.ReadAllText(Environment.CurrentDirectory + "\\settings\\" + "last_session" + ".txt"));
                }
                catch
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
            else
            { 
            comboBox1.SelectedIndex = 0;
        }

            //Create Settings Folder if missing
            if (!Directory.Exists(Environment.CurrentDirectory + "\\settings"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\settings");
            }
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
            if (comboBox1.Text == "WOD" || comboBox1.Text == "Legion" || comboBox1.Text == "BFA")
            {
                button1.Enabled = false;
                label1.Text = "Not implemented yet";
            }
            else
            {
                button1.Enabled = true;
                label1.Text = "";
            }
            //Console.WriteLine("Profile set to: " + comboBox1.Text);
            
            //Try to read the selectd profile path
            if (File.Exists(Environment.CurrentDirectory + "\\settings\\" + comboBox1.Text + ".txt"))
            {
                try
                {
                    textBox1.Text = File.ReadAllText(Environment.CurrentDirectory + "\\settings\\" + comboBox1.Text + ".txt");
                }
                catch
                {
                    throw new Exception("Something went wrong with: " + Environment.CurrentDirectory + "\\settings\\" + comboBox1.Text + ".txt");
                }
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //Check to see if there is a given path
            if (textBox1.Text != "" && comboBox1.Text != "")
            {
                //Save path for next boot:
                try
                {
                    File.WriteAllText(Environment.CurrentDirectory + "\\settings\\" + "last_session" + ".txt", comboBox1.SelectedIndex.ToString());
                }
                catch
                {
                    throw new Exception("Could not save this session index");
                }

                
                if (!File.Exists(Environment.CurrentDirectory + "\\settings\\" + comboBox1.Text + ".txt"))
                {
                    try
                    {
                        File.WriteAllText(Environment.CurrentDirectory + "\\settings\\" + comboBox1.Text + ".txt", textBox1.Text);
                    }
                    catch
                    {
                        throw new Exception("Could not create file: " + Environment.CurrentDirectory + "\\settings\\" + comboBox1.Text + ".txt");
                    }
                }

                //Load up the main GUI and set the profile acordingly
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
