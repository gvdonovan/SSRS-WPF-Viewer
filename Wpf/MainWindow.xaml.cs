using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.RS2010;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ReportingService2010 _ssrs;
        ReportTreeViewViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _ssrs = new ReportingService2010();
            _ssrs.Credentials = System.Net.CredentialCache.DefaultCredentials;

            _viewModel = new ReportTreeViewViewModel(_ssrs);
            

            base.DataContext = _viewModel;

            this._reportViewer.ProcessingMode = Microsoft.Reporting.WinForms.ProcessingMode.Remote;
            var rpt = _reportViewer.ServerReport;

            // Get a reference to the default credentials
            System.Net.ICredentials credentials = System.Net.CredentialCache.DefaultCredentials;

            // Get a reference to the report server credentials
            ReportServerCredentials rsCredentials = rpt.ReportServerCredentials;

            // Set the credentials for the server report
            rsCredentials.NetworkCredentials = credentials;

            // Set the report server URL and report path
            rpt.ReportServerUrl = new Uri("https://reports.elemetal.com/ReportServer_MIDAS/");
            rpt.ReportPath = "/Midas/Average Lots Per Day";
            _reportViewer.RefreshReport();

            _viewModel.Selected += (s, e) => 
            {
                rpt.ReportPath = e.Path;
                _reportViewer.RefreshReport();
            };

        }

        private void WindowsFormsHost_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ReportTreeViewViewModel
    {
        public event EventHandler<BiffEventArgs> Selected; 

        private ReportingService2010 _ssrs;
        readonly ReadOnlyCollection<ReportViewModel> _reports;

        public ReportTreeViewViewModel(ReportingService2010 ssrs)
        {
            _ssrs = ssrs;            

            var root = new ReportViewModel("Midas", "/Midas");

            BuildTree(root.Path, root);

            _reports = new ReadOnlyCollection<ReportViewModel>(new ReportViewModel[]{root});
    
        }

        private void BuildTree(string path, ReportViewModel parent)
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
                            var node = new ReportViewModel(item.Name, item.Path);
                            node.Selected += node_Selected;

                            //Add the node to the parent node collection                            
                            parent.Children.Add(node);

                            // recurse
                            if (item.TypeName == "Folder")
                                BuildTree(item.Path, node);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message));
            }
        }

        void node_Selected(object sender, BiffEventArgs e)
        {
            if (Selected != null) Selected(this, e);
        }

        public ReadOnlyCollection<ReportViewModel> Reports
        {
            get { return _reports; }
        }
    }

    public class ReportViewModel : INotifyPropertyChanged
    {
        public event EventHandler<BiffEventArgs> Selected; 

        bool _isExpanded;
        bool _isSelected;
        
        private  List<ReportViewModel> _children;
        readonly ReportViewModel _parent;
        public string Name { get; set; }
        public string Path { get; set; }

        public ReportViewModel(string name, string path) : this(name, path, null) {}

        public ReportViewModel(string name, string path, ReportViewModel parent)
        {            
            Name = name;
            Path = path;
            _parent = parent;
            _children = new List<ReportViewModel>();
        }

        public ReportViewModel Parent
        {
            get { return _parent; }
        }

        public List<ReportViewModel> Children
        {
            get { return _children; }
            set 
            { 
                _children = value; 
                OnPropertyChanged("Children"); 
            }
        }

        #region IsExpanded

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");

                    if (Selected != null) Selected(this, new BiffEventArgs(Path));
                }
            }
        }

        #endregion // IsSelected

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members        
    }    
}
