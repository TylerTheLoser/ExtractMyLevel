using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
//using System.IO.Compression;
using System.IO;
using Ionic.Zip;
using System.Threading;

namespace IngestMyLevel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            watch();
            string sAttr;
            sAttr = ConfigurationManager.AppSettings.Get("tprodir");

        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    UpdateSetting(fbd.SelectedPath.ToString());
                }
            }
        }

        private static void UpdateSetting(string value)
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove("tprodir");
            config.AppSettings.Settings.Add("tprodir", value);
            config.Save(ConfigurationSaveMode.Minimal);

        }

        public void ExtractFiles(string fileName, string outputDirectory)
        {
            try
            {
                using (ZipFile zip1 = ZipFile.Read(fileName))
                {
                    var selection = (from e in zip1.Entries
                                         //where (e.FileName).StartsWith(Path.GetFileName("/User/"))
                                         where (e.FileName).StartsWith("User/", StringComparison.OrdinalIgnoreCase)
                                     select e);

                    foreach (var e in selection)
                    {
                        Console.WriteLine(e);
                        string text1 = e.ToString();
                        txtStatus.Invoke(new Action(() => txtStatus.AppendText(text1)));
                        txtStatus.Invoke(new Action(() => txtStatus.AppendText(Environment.NewLine)));
                        e.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);

                    }
                }
            } catch(System.IO.IOException)
            {
                //txtLog.Text = "Exception Thrown...";
                return;
            }
        }
        FileSystemWatcher watcher;
        private void watch()
        {
            string sAttr;
            sAttr = ConfigurationManager.AppSettings.Get("tprodir");
            //FileSystemWatcher watcher = new FileSystemWatcher();
            this.watcher = new FileSystemWatcher();
            watcher.Path = sAttr;
            watcher.Filter = "*.zip";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;

        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //TODO add extraction
            Console.WriteLine("Found file: " + e.FullPath);
            string text = "Found file: " + e.FullPath;
            txtStatus.Invoke(new Action(() => txtStatus.AppendText(text)));
            txtStatus.Invoke(new Action(() => txtStatus.AppendText(Environment.NewLine)));
            var myfile = e.FullPath;
            string sAttr;
            sAttr = ConfigurationManager.AppSettings.Get("tprodir");
            ExtractFiles(myfile, sAttr); //extraction
            try
            {
                File.Delete(myfile); //delete the file
            } catch(System.IO.IOException) //this will be thrown because it's trying to delete the file while copying it
            {
                Thread.Sleep(10000); //gives it time to copy over... might have to adjust this
            }

        }

        public void Dispose()
        {
            watcher.Changed -= OnChanged;
            this.watcher.Dispose();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnFile_Click(object sender, EventArgs e) //debug
        {
            string sAttr;
            string fullPath;
            sAttr = ConfigurationManager.AppSettings.Get("tprodir");
            fullPath = Path.GetFullPath(sAttr);
            
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                ExtractFiles(openFileDialog1.FileName, fullPath);
            }
            
        }
    }
}
