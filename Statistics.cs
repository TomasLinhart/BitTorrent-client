using System.ComponentModel;

namespace BitTorrent_client
{
    public class Statistics : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _overallDownloadSpeed;
        private double _totalDownloaded;
        private double _overallUploadSpeed;
        private double _totalUploaded;
        private int _totalDhtNodes;

        public double OverallDownloadSpeed
        {
            get { return _overallDownloadSpeed; }
            set
            {
                _overallDownloadSpeed = value;
                OnPropertyChanged("OverallDownloadSpeed");
            }
        }
        public double TotalDownloaded
        {
            get { return _totalDownloaded; }
            set
            {
                _totalDownloaded = value;
                OnPropertyChanged("TotalDownloaded");
            }
        }
        public double OverallUploadSpeed
        {
            get { return _overallUploadSpeed; }
            set
            {
                _overallUploadSpeed = value;
                OnPropertyChanged("OverallUploadSpeed");
            }
        }
        public double TotalUploaded
        {
            get { return _totalUploaded; }
            set
            {
                _totalUploaded = value;
                OnPropertyChanged("TotalUploaded");
            }
        }
        public int TotalDhtNodes
        {
            get { return _totalDhtNodes; }
            set
            {
                _totalDhtNodes = value;
                OnPropertyChanged("TotalDhtNodes");
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
