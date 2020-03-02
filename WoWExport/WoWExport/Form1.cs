using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using WoWFormatLib.FileReaders;

namespace WoWExport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Set picturebox defaults
            panel1.AutoScroll = true;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            this.Text = "WoW Export";

            button1.Text = "Export";
            textBox1.Text = "D:\\export";

            groupBox1.Text = "Preview";

            //Debug button - to be deleted later
            button2.Hide();
            button2.Text = "Crash Me!";

            LoadGame();
        }

        //---------------------------------------------------------------------------
        //Export Button:
        //---------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Level != 0)
                {
                    Managers.ConfigurationManager.OutputDirectory = textBox1.Text + "//";
                    string selectedItem = treeView1.SelectedNode.FullPath;
                    selectedItem = selectedItem.Substring(5, selectedItem.Length - 5); //that's because we remove from the name "root\"

                    switch (Path.GetExtension(selectedItem))
                    {
                        case ".adt":
                            new WoWExport.Form_ADTExport(selectedItem).Show();
                            break;
                        case ".m2":
                            new WoWExport.Form_M2Export(selectedItem).Show();
                            break;
                        case ".wmo":
                            new WoWExport.Form_WMOExport(selectedItem).Show();
                            break;
                        case ".blp":
                            ExportBLP(selectedItem);
                            break;
                        default:
                            MessageBox.Show("Direct file export not supported yet");
                            break;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        //Debug Button:
        //---------------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL ADT:
            //---------------------------------------------------------------------------
            //ADTReader reader = new ADTReader();
            //reader.LoadADT(@"world\maps\azeroth\azeroth_30_47.adt");
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL M2:
            //---------------------------------------------------------------------------
            //M2Reader reader = new M2Reader();
            //reader.LoadM2(@"");
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL WMO:
            //---------------------------------------------------------------------------
            //WMOReader reader = new WMOReader();
            //reader.LoadWMO(@"");
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL ADT (335):
            //---------------------------------------------------------------------------
            //ADTReader reader = new ADTReader();
            //reader.Load335ADT(@"world\maps\azeroth\azeroth_32_48.adt");
            //---------------------------------------------------------------------------
            var list = new List<string>();
            ListCheckedFiles(treeView1.Nodes, list);
            for(int i = 0; i < list.Count; i++)
            {
                Console.WriteLine(list[i]);
            }
        }

        private void ExportBLP(string file)
        {
            if (!File.Exists(Path.Combine(textBox1.Text, Path.GetFileNameWithoutExtension(file) + ".png")))
            {
                try
                {
                    BLPReader reader = new BLPReader();
                    reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(file));
                    reader.bmp.Save(Path.Combine(textBox1.Text, Path.GetFileNameWithoutExtension(file) + ".png"));
                }
                catch
                {
                    Console.WriteLine("Error occured on saving the file: " + file);
                    Console.WriteLine(Path.Combine(textBox1.Text, Path.GetFileNameWithoutExtension(file) + ".png"));
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL ADT:
            //---------------------------------------------------------------------------
            //ADTReader reader = new ADTReader();
            //reader.LoadADT(@"world\maps\azeroth\azeroth_30_47.adt");
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL M2:
            //---------------------------------------------------------------------------
            //M2Reader reader = new M2Reader();
            //reader.LoadM2(@"");
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL WMO:
            //---------------------------------------------------------------------------
            //WMOReader reader = new WMOReader();
            //reader.LoadWMO(@"");
            //---------------------------------------------------------------------------

            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL ADT (335):
            //---------------------------------------------------------------------------
            //ADTReader reader = new ADTReader();
            //reader.Load335ADT(@"world\maps\azeroth\azeroth_32_48.adt");
            //---------------------------------------------------------------------------
        }


        private TreeNode PopulateTreeNode2(List<string> paths, string pathSeparator)
        {
            if (paths == null)
                return null;

            TreeNode thisnode = new TreeNode();
            TreeNode currentnode;
            char[] cachedpathseparator = pathSeparator.ToCharArray();
            foreach (string path in paths)
            {
                currentnode = thisnode;
                foreach (string subPath in path.Split(cachedpathseparator))
                {
                    if (null == currentnode.Nodes[subPath])
                        currentnode = currentnode.Nodes.Add(subPath, subPath);
                    else
                        currentnode = currentnode.Nodes[subPath];
                }
            }
            return thisnode;
        }


        private void LoadGame()
        {
            //Extract listfiles to cache
            if (!Directory.Exists(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles");
            }
            Managers.ArchiveManager.FindLocale();
            Managers.ArchiveManager.ExtractListfiles(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles\\");

            Managers.ArchiveManager.LoadArchives();
            Managers.ArchiveManager.GenerateMainListFileFromMPQ();

            if (Managers.ConfigurationManager.Profile == "LK" || Managers.ConfigurationManager.Profile == "TBC" || Managers.ConfigurationManager.Profile == "Vanilla")
            {
                Managers.md5Manager.LoadMD5();
            }

            Generators.DisplayStructure.GenerateList();
            treeView1.Nodes.Add(PopulateTreeNode2(Generators.DisplayStructure.MLF, "\\"));
            treeView1.Nodes[0].Expand();
            treeView1.Nodes[0].Text = "root";
            treeView1.Sort();

            Managers.ConfigurationManager.ADTExportM2 = true;
            Managers.ConfigurationManager.ADTExportWMO = true;
            Managers.ConfigurationManager.ADTExportFoliage = false; //Obsolete atm
            Managers.ConfigurationManager.ADTexportTextures = true;
            Managers.ConfigurationManager.ADTexportAlphaMaps = true;
            Managers.ConfigurationManager.WMOExportM2 = true;
            //Managers.ConfigurationManager.OutputDirectory = textBox1.Text + "//"; -- not here please
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.FullPath.EndsWith(".blp"))
            {
                try
                {
                    BLPReader reader = new BLPReader();
                    reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(treeView1.SelectedNode.FullPath.Replace(@"root\", "")));
                    pictureBox1.Image = reader.bmp;
                }
                catch
                {
                    Console.WriteLine("Error occured while trying to read " + treeView1.SelectedNode.FullPath.Replace(@"root\", ""));
                }
            }

            if (treeView1.SelectedNode.FullPath.EndsWith(".adt"))
            {
                try
                {
                    if (Managers.ConfigurationManager.Profile == "Cata" || Managers.ConfigurationManager.Profile == "MOP")
                    {
                        string filedirectory = treeView1.SelectedNode.FullPath.Replace("\\maps\\", "\\minimaps\\");
                        filedirectory = filedirectory.Substring(0, filedirectory.LastIndexOf("\\") + 1);
                        filedirectory = filedirectory.Substring(5, filedirectory.Length - 5);

                        string filename = Path.GetFileNameWithoutExtension(treeView1.SelectedNode.FullPath);
                        filename = filename.Replace(filename.Substring(0, filename.IndexOf("_") + 1), "map") + ".blp";
                        //Console.WriteLine(filedirectory + filename);
                        try
                        {
                            BLPReader reader = new BLPReader();
                            reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(filedirectory + filename));
                            pictureBox1.Image = reader.bmp;
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        string filename = treeView1.SelectedNode.FullPath;
                        try
                        {
                            BLPReader reader = new BLPReader();
                            reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(Managers.md5Manager.TranslateThisMap(filename)));
                            pictureBox1.Image = reader.bmp;
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.treeview.aftercheck?view=netframework-4.0
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // The code only executes if the user caused the checked state to change.
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    /* Calls the CheckAllChildNodes method, passing in the current 
                    Checked value of the TreeNode whose checked state changed. */
                    this.CheckAllChildNodes(e.Node, e.Node.Checked);
                }
            }
        }

        // Updates all child tree nodes recursively.
        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;
                if (node.Nodes.Count > 0)
                {
                    // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                    this.CheckAllChildNodes(node, nodeChecked);
                }
            }
        }

        void ListCheckedFiles(TreeNodeCollection nodes, List<string> list)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                {
                    list.Add(node.FullPath.Substring(5, node.FullPath.Length - 5));
                }
                ListCheckedFiles(node.Nodes, list);
            }
        }
    }
}
