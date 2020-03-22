using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using WoWFormatLib.FileReaders;
using System.ComponentModel;

namespace WoWExport
{
    public partial class Form1 : Form
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public TreeNode displayStructure = new TreeNode();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;

            //Set picturebox defaults
            panel1.AutoScroll = true;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            this.Text = "WoW Export";

            button1.Text = "Export";
            button1.Enabled = false;
            textBox1.Text = @"D:\export";
            textBox1.Enabled = false;

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
            var list = new List<string>();
            ListCheckedFiles(treeView1.Nodes, list);
            Managers.ConfigurationManager.OutputDirectory = textBox1.Text + "//";

            if (list.Count == 0)
            {
                if (treeView1.SelectedNode != null)
                {
                    if (treeView1.SelectedNode.Level != 0)
                    {
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
                                Exporters.BLPExporter.ExportBLP(selectedItem, Managers.ConfigurationManager.OutputDirectory);
                                break;
                            default:
                                MessageBox.Show("Direct file export not supported yet");
                                break;
                        }
                    }
                }
            }
            else
            {
                new WoWExport.Form_BatchExport(list).Show();
            }
        }
        //---------------------------------------------------------------------------
        //Debug Button:
        //---------------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            //---------------------------------------------------------------------------
            //TEST INDIVIDUAL CASC PREVIEW TEXTURE:
            //---------------------------------------------------------------------------
            /*
            string fileToLoad = @"tileset\barrens\barrensbasedirt.blp"
            if (Managers.ArchiveManager.cascHandler.FileExists(fileToLoad))
            {

                Console.WriteLine("File exists");
                Console.WriteLine(fileToLoad);
                try
                {
                    using (Stream stream = Managers.ArchiveManager.cascHandler.OpenFile(fileToLoad))
                    {
                        BLPReader reader = new BLPReader();
                        reader.LoadBLP(stream);
                        pictureBox1.Image = reader.bmp;
                    }
                }
                catch (Exception ef)
                {
                    Console.WriteLine(ef.Message);
                }
            }
            else
            {
                Console.WriteLine("File not found");
            }
            */
            //---------------------------------------------------------------------------

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
            worker.RunWorkerAsync();
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
                    if (Path.GetExtension(node.FullPath) != "")
                    {
                        list.Add(node.FullPath.Substring(5, node.FullPath.Length - 5));
                    }
                }
                ListCheckedFiles(node.Nodes, list);
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Managers.ArchiveManager.usingCasc)
            {
                worker.ReportProgress(0, "Loading CASC");
                Console.WriteLine("Loading CASC");
                Managers.ArchiveManager.LoadCASC();
            }
            else
            {
                Console.WriteLine("Loading MPQ");
                worker.ReportProgress(0, "Searching cached listfiles");
                //Extract listfiles to cache
                if (!Directory.Exists(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles"))
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles");
                }
                worker.ReportProgress(1, "Finding locale");
                Managers.ArchiveManager.FindLocale();
                worker.ReportProgress(2, "Extracting listfiles if needed");
                Managers.ArchiveManager.ExtractListfiles(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles\\");
                worker.ReportProgress(3, "Loading game archives");
                Managers.ArchiveManager.LoadArchives();
                worker.ReportProgress(4, "Merging listfiles");
                Managers.ArchiveManager.GenerateMainListFileFromMPQ();

                if (Managers.ConfigurationManager.Profile == "LK" || Managers.ConfigurationManager.Profile == "TBC" || Managers.ConfigurationManager.Profile == "Vanilla")
                {
                    worker.ReportProgress(5, "Loading MD5 minimap translator");
                    Managers.md5Manager.LoadMD5();
                }

                worker.ReportProgress(6, "Generating display list... please wait (window may freez)");
                Generators.DisplayStructure.GenerateList();

                PopulateTree();

                worker.ReportProgress(99, "Assigning settings");
                Managers.ConfigurationManager.ADTExportM2 = true;
                Managers.ConfigurationManager.ADTExportWMO = true;
                Managers.ConfigurationManager.ADTExportFoliage = false; //Obsolete atm
                Managers.ConfigurationManager.ADTexportTextures = true;
                Managers.ConfigurationManager.ADTexportAlphaMaps = true;
                Managers.ConfigurationManager.WMOExportM2 = true;
                //Managers.ConfigurationManager.OutputDirectory = textBox1.Text + "//"; -- not here please
            }

        }

        private void PopulateTree()
        {
            treeView1.Invoke(new MethodInvoker(delegate
            {
                treeView1.Nodes.Add(PopulateTreeNode2(Generators.DisplayStructure.MLF, "\\"));
                treeView1.Nodes[0].Text = "root";
                treeView1.Nodes[0].Expand();
            }));
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            textBox1.Enabled = true;
            label1.Hide();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                label1.Text = state;
            }
            //progressBar.Value = e.ProgressPercentage;
        }
    }
}
