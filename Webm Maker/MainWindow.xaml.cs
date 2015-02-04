using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections;

namespace Webm_Maker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string currentdir = Directory.GetCurrentDirectory();

        // bunch of directories
        string logfile;
        string filein;
        string fileout;
        string ffmpegdir;

        // user params
        int qmax = 0;
        double resolution = 1;
        double filesizelimit;
        string audio;
        string writelog;
        bool showconsole;

        FileStream fs;
        Process p;

        public MainWindow()
        {
            InitializeComponent();
            Thread myThread = new Thread(new ThreadStart(checkFields));
            myThread.Start();
        }
        
        // kill ffmpeg if window closed early
        private void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            Process[] p = Process.GetProcessesByName("ffmpeg");
            if(p.Length > 0)
            {
                for(int i = 0; i < p.Length; i++)
                {
                    p[i].Kill();
                }
            }
            if(File.Exists(logfile))
            {
                fs.Close();
                File.Delete(logfile);
            }
        }

        // check if all fields are filled in for status indicator
        public void checkFields()
        {
            try
            {
                bool ready = false;
                while (!ready)
                {
                    this.Dispatcher.Invoke(new Action(() => ready = checkReady()));
                    System.Threading.Thread.Sleep(100);
                }
                this.Dispatcher.Invoke(readyText);
            }
            catch (TaskCanceledException e)
            {
                Thread.CurrentThread.Abort();
            }
        }

        // function for the above one
        public bool checkReady()
        {
            bool ready = false;
            if ((Save_Textbox.Text != "") && (Browse_Textbox.Text != "") && (FilesizeLimit.Text != "") && (FilesizeLimit.Text != "0"))
            {
                ready = true;
            }
            return ready;
        }

        // another one for checkFields
        public void readyText()
        {
            Label.Content = "Ready to convert.";
            ConvertButton.IsEnabled = true;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".mp4";
            dlg.Filter = "Supported Formats|*.mp4;*.avi;*.mkv;*.mov;*.flv;*.gif;*.webm";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                Browse_Textbox.Text = filename;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.FileName = "output"; // Default file name
            dlg.DefaultExt = ".webm"; // Default file extension
            dlg.Filter = "WebM Video (*.webm)|*.webm"; // Filter files by extension 

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // Save document 
                string filename = dlg.FileName;
                Save_Textbox.Text = filename;
            }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            ConvertButton.IsEnabled = false;

            ffmpegdir = currentdir + "\\bin\\ffmpeg.exe";
            filein = Browse_Textbox.Text;
            fileout = Save_Textbox.Text;
            logfile = currentdir + "\\bin\\done.txt";
            filesizelimit = Convert.ToDouble(FilesizeLimit.Text) * 1000000;

            // for audio
            if((bool) Audio.IsChecked)
            {
                audio = "";
            }
            else
            {
                audio = "-an";
            }

            // for console
            if ((bool)ConsoleOut.IsChecked)
            {
                showconsole = true;
            }
            else
            {
                showconsole = false;
            }

            // for logging
            if ((bool)WriteLog.IsChecked)
            {
                writelog = "-report";
            }
            else
            {
                writelog = "";
            }

            // delete existing files
            if (File.Exists(fileout))
            {
                File.Delete(fileout);
            }
            if (File.Exists(logfile))
            {
                File.Delete(logfile);
            }
            convert(ffmpegdir, filein, fileout);
        }

        public void convert(string ffmpegdir, string sourcefile, string fileout)
        {
            qmax += 10;
            if(qmax > 30)
            {
                qmax = 10;
                resolution -= 0.1;
            }

            this.Dispatcher.Invoke(progressLabel);

            p = new Process();

            if(showconsole)
            {
                p.StartInfo.CreateNoWindow = false;
            }
            else
            {
                p.StartInfo.CreateNoWindow = true;
            }
            p.StartInfo.FileName = ffmpegdir;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = "-i \"" + sourcefile + "\" -vf scale=iw*" + resolution + ":ih*" + resolution + ",colormatrix=bt709:bt601 -pix_fmt yuv420p -threads 8 -qmin 0 -qmax " + qmax + " -c:v vp8 " + audio + " -y \"" + fileout + "\" " + writelog;
            p.Start();

            // log stuff
            Thread logThread = new Thread(new ThreadStart(writeLog));
            logThread.Start();

            //p.StandardInput.WriteLine("cd /d " + ffmpegdir);
            //p.StandardInput.WriteLine("ffmpeg -i \"" + sourcefile + "\" -vf scale=iw*" + resolution + ":ih*" + resolution + ",colormatrix=bt709:bt601 -pix_fmt yuv420p -threads 8 -qmin 0 -qmax " + qmax + " -c:v vp8 " + audio + " -y \"" + fileout + "\" 2> done.txt");
            Thread checkFileThread = new Thread(new ThreadStart(checkFile));
            checkFileThread.Start();
        }

        public void writeLog()
        {
            Console.WriteLine(p.StandardOutput.ReadToEnd());
            fs = File.Create(logfile);
            StreamWriter sw = new StreamWriter(fs);
            Console.SetOut(sw);
            fs.Close();
        }

        public void progressLabel()
        {
            Label.Content = "Converting with resolution factor " + resolution + " and qmax at " + qmax + ".";
        }

        // to check if filesize has been reached
        public void checkFile()
        {
            bool done = false;
            FileInfo log = new FileInfo(logfile);
            while (!done)
            {
                if (File.Exists(logfile) && !IsFileLocked(log))
                {
                    done = true;
                    fs.Close();
                    File.Delete(logfile);
                }
                System.Threading.Thread.Sleep(500);
            }
            FileInfo output = new FileInfo(fileout);
            if (output.Length > filesizelimit)
            {
                convert(ffmpegdir, filein, fileout);
            }
            else if ((output.Length < filesizelimit) && (output.Length != 0) && !IsFileLocked(output))
            {
                this.Dispatcher.Invoke(setLabel);
            }

        }

        // call when done
        public void setLabel()
        {
            Label.Content = "Done!";
            ConvertButton.IsEnabled = true;
            qmax = 0;
            resolution = 1;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
