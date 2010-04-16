using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MonoTorrent.Client;

namespace BitTorrent_client
{
    public class TorrentData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _torrentName;
        private long _size;
        private double _progress;
        private TorrentStatus _status;
        private int _seeds;
        private int _peers;
        private double _downloadSpeed;
        private double _uploadSpeed;

        public TorrentHash Hash
        {
            get; set;
        }

        public string TorrentName
        {
            get { return _torrentName; }
            set
            {
                _torrentName = value;
                OnPropertyChanged("TorrentName");
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public long Size
        {
            get { return _size/1024/1024; }
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

        public TorrentStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public int Seeds
        {
            get { return _seeds; }
            set
            {
                _seeds = value;
                OnPropertyChanged("Seeds");
            }
        }

        public int Peers
        {
            get { return _peers; }
            set
            {
                _peers = value;
                OnPropertyChanged("Peers");
            }
        }

        public double DownloadSpeed
        {
            get { return _downloadSpeed/1024; }
            set
            {
                _downloadSpeed = value;
                OnPropertyChanged("DownloadSpeed");
            }
        }

        public double UploadSpeed
        {
            get { return _uploadSpeed/1024; }
            set
            {
                _uploadSpeed = value;
                OnPropertyChanged("UploadSpeed");
            }
        }

        private void OnPropertyChanged(string name)
        {
            var propertyChangedEventHandler = PropertyChanged;
            if (propertyChangedEventHandler != null)
            {
                propertyChangedEventHandler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
