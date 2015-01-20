using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Windows.RS2010;

namespace Windows
{
    public partial class Form1 : Form
    {
        private ReportingService2010 _ssrs;
        private ImageList _images;

        public Form1()
        {
            InitializeComponent();

            _images = new ImageList();
            _images.Images.Add(Image.FromFile("Images/folder.gif"));
            _images.Images.Add(Image.FromFile("Images/report.gif"));
            treeView1.ImageList = _images;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _ssrs = new ReportingService2010();
            _ssrs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            BuildTree();
        }
        
        private void BuildTree()
        {

            var root = new TreeNode();
            // give root node a name
            root.Text = "Midas";
            root.ExpandAll();
            // add node to treeview object
            treeView1.Nodes.Add(root);
            // populate treeview with catalog items
            GetCatalogItems("/Midas", root);    

        }

        private void GetCatalogItems(string path, TreeNode parentNode)
        {
            CatalogItem[] items;

            try
            {
                // if catalog path is empty, use root, if not pass the folde path
                if (path.Length == 0)
                {
                    items = _ssrs.ListChildren("/", false); // no recursion (false)
                }
                else
                {
                    items = _ssrs.ListChildren(path, false); // no recursion (false)
                }
            
                // iterate through catalog items and populate treeview control
                foreach (CatalogItem item in items)
                {
                    //If folder is hidden, skip it
                    if (item.Hidden != true)
                    {
                        // ensure only folders are rendered
                        if ((item.TypeName != "DataSource"
                            & item.Name != "Data Sources" & item.TypeName != "Model"
                            & item.Name != "Models"))
                        {
                            var node = new TreeNode(item.Name);
                            node.Text = item.Name;
                            node.Tag = item.Path;
                            node.ImageIndex = item.TypeName == "Folder" ? 0 : 1;
                            node.SelectedImageIndex = item.TypeName == "Folder" ? 0 : 1;
                            
                            
                            //Add the node to the parent node collection                            
                            parentNode.Nodes.Add(node);
                            // recurse
                            if (item.TypeName == "Folder")
                                GetCatalogItems(item.Path, node);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
        }
    }
}
