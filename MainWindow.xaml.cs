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
        /// <summary>
        /// Field for currently show torrents.
        /// </summary>
        private TorrentStatus[] _currentFilter;

        /// <summary>
        /// Collection property of all currenly open torrents 
        /// </summary>
        public ObservableCollection<TorrentData> TorrentCollection { get; private set; }
        /// <summary>
        /// Collection property of all currenly open torrents 
        /// </summary>
        public ObservableCollection<TorrentData> FilteredCollection { get; private set; } // TODO: Has to be private? Even field?

        public MainWindow()
        {
            TorrentCollection = new ObservableCollection<TorrentData>();
            FilteredCollection = new ObservableCollection<TorrentData>();
            InitializeComponent();
            _currentFilter = new[] { TorrentStatus.Downloading, TorrentStatus.Seeding, TorrentStatus.Stopped, TorrentStatus.Hashing };
            TorrentCollection.CollectionChanged += OnTorrentCollectionChanged;

            startButton.Click += OnStartButtonClick;

            listBox.SelectionChanged += OnListBoxSelectionChanged;

            #region TESTING
            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 3, 0) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
            SetTestData();
            #endregion
        }

        private void OnStartButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        #region FOR TESTING ONLY
        private void SetTestData()
        {
            TorrentCollection.Add(new TorrentData { TorrentName = "HashingTest", Status = TorrentStatus.Hashing });
            TorrentCollection.Add(new TorrentData { TorrentName = "DownloadingTest", Status = TorrentStatus.Downloading });
            TorrentCollection.Add(new TorrentData { TorrentName = "StoppedTest", Status = TorrentStatus.Stopped });
            TorrentCollection.Add(new TorrentData { TorrentName = "SeedingTest", Status = TorrentStatus.Seeding });
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            TorrentCollection.Add(new TorrentData { TorrentName = "NewlyAdded...", Status = TorrentStatus.Stopped });
            TorrentCollection.First().TorrentName = "ChangedName";
            TorrentCollection.First().Priority = 3;
            TorrentCollection.First().Status = TorrentStatus.Hashing;
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

        private void OnTorrentCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FilterCollection();
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBoxItem = (((ListBox)sender).SelectedItem as ListBoxItem);
            if (listBoxItem != null)
            {
                // TODO: Am i doing it right?
                switch (listBoxItem.Content.ToString())
                {
                    case "Downloading":
                        _currentFilter = new[] { TorrentStatus.Downloading };
                        break;
                    case "Completed":
                        _currentFilter = new[] { TorrentStatus.Seeding };
                        break;
                    case "Active":
                        _currentFilter = new[] { TorrentStatus.Seeding, TorrentStatus.Downloading};
                        break;
                    case "Inactive":
                        _currentFilter = new[] { TorrentStatus.Stopped };
                        break;
                    default:
                        _currentFilter = new[] { TorrentStatus.Downloading, TorrentStatus.Seeding, TorrentStatus.Stopped, TorrentStatus.Hashing };
                        break;
                }
            }
            FilterCollection();
        }
    }
}
