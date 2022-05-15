using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CHDUI
{
    public class FileHandler : INotifyPropertyChanged
    {
        private int current;

        private string selectedoutputfolder = "No output folder Selected.",
                       outputdata,
                       errordata;

        private bool chdmanpresent,
                     working,
                     outputfoldervalid;

        public FileHandler()
        {
            working = false;
            outputfoldervalid = false;
            FileEntries = new();
            chdmanpresent = File.Exists($"{Directory.GetCurrentDirectory()}\\chdman.exe");
            outputdata += chdmanpresent ? "chdman.exe found.\r\n" : "chdman.exe missing!\r\n";
            UpdateProperties();
        }

        public void Clear()
        {
            FileEntries.Clear();
            current = 0;
            UpdateProperties();
        }

        public void FromFileDrop(string[] FileNames)
        {
            IterateFiles(FileNames);
            UpdateProperties();
        }

        public int FileCount
        {
            get
            {
                return FileEntries.Count;
            }
        }

        public string FileProgress
        {
            get
            {
                return FileCount > 0 ? $"File: {(current + 1 < FileCount ? current + 1 : current)} of {FileCount}" : "No files.";
            }
        }

        public string SelectedOutputFolder
        {
            get
            {
                return selectedoutputfolder;
            }
            set
            {
                outputfoldervalid = Directory.Exists(value);
                selectedoutputfolder = outputfoldervalid ? value : "No output folder Selected.";
                OnPropertyChanged(nameof(SelectedOutputFolder));
                UpdateProperties();
            }
        }

        public int Current { get { return current; } }

        public bool ButtonGoEnabled { get { return FileCount > 0 && !working && outputfoldervalid && chdmanpresent;  } }

        public bool ButtonClearEnabled { get { return FileCount > 0 && !working && chdmanpresent; } }

        public bool ButtonOutputFolderEnabled { get { return !working && chdmanpresent; } }

        public ObservableCollection<FileEntry> FileEntries { get; private set; }

        public string ProcessOutput
        {
            get
            {
                return $"{outputdata}\r\n{errordata}\r\n";
            }
        }

        public void ConvertFiles(int item = 0)
        {
            if (!working) { working = true; }

            UpdateProperties();
            Process p = new();
            p.StartInfo.FileName = "chdman.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.EnableRaisingEvents = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.OutputDataReceived += CHDMan_OutputDataReceived;
            p.StartInfo.RedirectStandardError = true;
            p.ErrorDataReceived += CHDMan_ErrorDataReceived;
            p.Exited += CHDMan_Exited;

            FileEntries[item].Activity = FileEntryActivity.BUSY;
            FileEntries[item].Result = FileEntryResult.WAITING;

            string filename = Path.GetFileNameWithoutExtension(FileEntries[item].FileInfo.Name);
            string outputdir = $"{SelectedOutputFolder}\\{filename}";
            _ = Directory.CreateDirectory(outputdir);
            p.StartInfo.Arguments = $"createcd -i \"{FileEntries[item].FileInfo.FullName}\" -o \"{outputdir}\\{filename}.chd\"";
            _ = p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
        }

        private void CHDMan_Exited(object sender, EventArgs e)
        {
            Process p = sender as Process;
            FileEntries[current].Activity = FileEntryActivity.DONE;
            FileEntries[current].Result = p.ExitCode == 0 ? FileEntryResult.COMPLETED : FileEntryResult.FAILED;
            outputdata += $"{errordata}\r\n";
            p.OutputDataReceived -= CHDMan_OutputDataReceived;
            p.ErrorDataReceived -= CHDMan_ErrorDataReceived;
            p.Exited -= CHDMan_Exited;

            current++;
            if (current <= FileCount - 1) { ConvertFiles(current); }
            if (current > FileCount - 1)
            {
                working = false;
                errordata = "";
                UpdateProperties();
            }
        }

        private void CHDMan_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data)) { errordata = $"{e.Data}\r\n"; }
            OnPropertyChanged(nameof(ProcessOutput));
        }

        private void CHDMan_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            outputdata += $"{e.Data}\r\n";
            OnPropertyChanged(nameof(ProcessOutput));
        }

        private void IterateFiles(string[] FileNames)
        {
            foreach (string f in FileNames)
            {
                FileInfo fi = new(f);
                if ((fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory) { IterateFiles(EnumerateDirectory(f)); }
                else { FileEntries.Add(new(new(f))); }
            }
        }

        private void IterateFiles(FileInfo[] Files)
        {
            foreach (FileInfo fi in Files)
            {
                if ((fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory) { IterateFiles(EnumerateDirectory(fi.FullName)); }
                else { FileEntries.Add(new(fi)); }
            }
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(FileCount));
            OnPropertyChanged(nameof(Current));
            OnPropertyChanged(nameof(ButtonGoEnabled));
            OnPropertyChanged(nameof(ButtonClearEnabled));
            OnPropertyChanged(nameof(FileProgress));
            OnPropertyChanged(nameof(ProcessOutput));
        }

        private static FileInfo[] EnumerateDirectory(string Path)
        {
            return (from file in new DirectoryInfo(Path).EnumerateFiles("*", SearchOption.AllDirectories)
                    where file.Exists && (file.Extension == ".cue" || file.Extension == ".iso")
                    select file).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string Name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }
    }
}