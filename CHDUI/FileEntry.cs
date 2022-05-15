using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace CHDUI
{
    public class FileEntry : INotifyPropertyChanged
    {
        private FileInfo fi;
        private FileEntryActivity fea;
        private FileEntryResult fer;

        public FileEntry(FileInfo FileInfo)
        {
            this.FileInfo = FileInfo;
            Activity = FileEntryActivity.NONE;
            Result = FileEntryResult.WAITING;
        }

        public FileInfo FileInfo
        {
            get
            {
                return fi;
            }
            private set
            {
                fi = value;
                OnPropertyChanged(nameof(FileInfo));
            }
        }

        public FileEntryActivity Activity
        {
            get
            {
                return fea;
            }
            set
            {
                fea = value;
                OnPropertyChanged(nameof(Activity));
            }
        }

        public FileEntryResult Result
        {
            get
            {
                return fer;
            }
            set
            {
                fer = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string Name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }
    }
}