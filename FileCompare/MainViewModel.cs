using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace FileCompare
{
    public class MainViewModel
    {
        public FileSearchCriteria FileSearchCriteria { get; set; }

        public MainViewModel()
        {
            FileSearchCriteria = LoadSearch();
            if (FileSearchCriteria == null) FileSearchCriteria = new FileSearchCriteria();

            SearchedFiles = new ObservableCollection<FileInfo>();

            // var fi = new FileInfo("D:\\Workarea\\Projects\\Essilor\\Sunix\\Sources\\Sunix\\EvisionModules\\Sunix.Optometrical.Web\\Helpers\\DynamicEntityQuery.cs");
        }

        private FileSearchCriteria LoadSearch()
        {
            string criteria = global::FileCompare.Properties.Settings.Default.FileSearchCriteria;

            if (string.IsNullOrWhiteSpace(criteria)) return null;

            XmlSerializer ser = new XmlSerializer(typeof(FileSearchCriteria));

            using (TextReader reader = new StringReader(criteria))
            {
                FileSearchCriteria fileSearchCriteria = ser.Deserialize(reader) as FileSearchCriteria;

                return fileSearchCriteria;
            }
        }

        ICommand searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                {
                    searchCommand = new DelegateCommand(OnSearch);
                }

                return searchCommand;
            }
        }

        ICommand copytoTempFolderCommand;
        public ICommand CopytoTempFolderCommand
        {
            get
            {
                if (copytoTempFolderCommand == null)
                {
                    copytoTempFolderCommand = new DelegateCommand(OnCopytoTempFolder);
                }

                return copytoTempFolderCommand;
            }
        }
        public ObservableCollection<FileInfo> SearchedFiles { get; set; }

        private void ProcessDirectory(string directory)
        {
            Regex searchPattern = new Regex(FileSearchCriteria.FileCardExtensions, RegexOptions.IgnoreCase);
            string[] files = System.IO.Directory.GetFiles(directory).Where(x => searchPattern.IsMatch(x)).ToArray();

            // Now we need to go through these files and make sure DateModified > specified date
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastWriteTime >= FileSearchCriteria.DateStart) SearchedFiles.Add(fi);
            }

            string[] directories = System.IO.Directory.GetDirectories(directory);
            var alldirectories = directories;
            if(! string.IsNullOrWhiteSpace( FileSearchCriteria.SkipSubDirectories))
                alldirectories = directories.Where(x => FileSearchCriteria.SkipSubDirectories.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Contains((new System.IO.DirectoryInfo(x)).Name) == false).ToArray();

            foreach (string dir in alldirectories)
                ProcessDirectory(dir);
        }

        private void OnSearch()
        {
            SaveSearch();

            SearchedFiles.Clear();
            ProcessDirectory(FileSearchCriteria.Directory);

            //OnCopytoTempFolder();
        }

        private void OnCopytoTempFolder()
        {
            try
            {
                // Copy results to clipboard
                StringBuilder sb = new StringBuilder();
                SearchedFiles.ToList().ForEach(x => sb.Append(x.FullName + ";"));
                System.Windows.Clipboard.SetText(sb.ToString());

                string sTempDirectory = Path.Combine(global::FileCompare.Properties.Settings.Default.TempDirectory.ToString(), "FileCompare_" + DateTime.Now.ToString("ddMMyyyyHHmmss"));

                if (!Directory.Exists(sTempDirectory))
                    Directory.CreateDirectory(sTempDirectory);

                foreach (var fi in SearchedFiles)
                {
                    string newTempDirectory = Path.Combine(sTempDirectory, fi.DirectoryName.Replace(FileSearchCriteria.Directory, ""));
                    if (!Directory.Exists(newTempDirectory))
                        Directory.CreateDirectory(newTempDirectory);

                    System.Windows.MessageBox.Show("Temp folder: " + newTempDirectory, "Temp Directory");

                    fi.CopyTo(Path.Combine(newTempDirectory, fi.Name));
                }

                System.Diagnostics.Process.Start(sTempDirectory);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace + "\n" + (ex.InnerException != null ? ex.InnerException.Message : string.Empty), "Error Occured", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
            }
        }

        private void SaveSearch()
        {
            XmlSerializer ser = new XmlSerializer(typeof(FileSearchCriteria));
            StringBuilder sbuilder = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sbuilder);

            ser.Serialize(writer, FileSearchCriteria);

            global::FileCompare.Properties.Settings.Default.FileSearchCriteria = sbuilder.ToString();
            global::FileCompare.Properties.Settings.Default.Save();
        }
    }
}
