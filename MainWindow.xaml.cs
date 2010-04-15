using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BitTorrent_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DispatcherTimer _timer;
        private TorrentStatus[] _currentFilter;

        public ObservableCollection<TorrentData> TorrentCollection { get; private set; }
        public ObservableCollection<TorrentData> FilteredCollection { get; private set; }

        public MainWindow()
        {
            TorrentCollection = new ObservableCollection<TorrentData>();
            FilteredCollection = new ObservableCollection<TorrentData>();
            InitializeComponent();
            _currentFilter = new[] { TorrentStatus.Finished, TorrentStatus.Paused };
            TorrentCollection.CollectionChanged += OnTorrentCollectionChanged;

            listBox.SelectionChanged += OnListBoxSelectionChanged;

            #region TESTING
            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 3, 0) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
            SetTestData();
            #endregion
        }
        #region FOR TESTING ONLY
        private void SetTestData()
        {
            TorrentCollection.Add(new TorrentData { TorrentName = "test", Status = TorrentStatus.Finished });
            TorrentCollection.Add(new TorrentData { TorrentName = "test", Status = TorrentStatus.Paused });
            TorrentCollection.Add(new TorrentData { TorrentName = "test", Status = TorrentStatus.Stopped });
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TorrentCollection.Add(new TorrentData { TorrentName = "hi", Status = TorrentStatus.Paused });
            TorrentCollection.First().TorrentName = "qwer";
            TorrentCollection.First().Priority = 3;
            TorrentCollection.First().Status = TorrentStatus.Finished;
            TorrentCollection.First().Progress = 7.5;

        }
        #endregion

        private void FilterCollection()
        {
            FilteredCollection.Clear();
            foreach (var torrentData in from torrentData in TorrentCollection
                                        from torrentStatus in _currentFilter
                                        where torrentData.Status == torrentStatus
                                        select torrentData)
            {
                FilteredCollection.Add(torrentData);
            }
        }

        void OnTorrentCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FilterCollection();
        }

        void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBoxItem = (((ListBox)sender).SelectedItem as ListBoxItem);
            if (listBoxItem != null)
            {
                switch (listBoxItem.Content.ToString())
                {
                    case "Downloading":
                        _currentFilter = new[] { TorrentStatus.Paused };
                        break;
                    case "Completed":
                        _currentFilter = new[] { TorrentStatus.Paused };
                        break;
                    case "Active":
                        _currentFilter = new[] { TorrentStatus.Paused };
                        break;
                    case "Inactive":
                        _currentFilter = new[] { TorrentStatus.Paused };
                        break;
                    default:
                        _currentFilter = new[] { TorrentStatus.Finished, TorrentStatus.Paused, TorrentStatus.Stopped };
                        break;
                }
            }
            FilterCollection();
        }
    }
}
