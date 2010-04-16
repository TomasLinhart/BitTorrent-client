using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Collections.Generic;

namespace BitTorrent_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DispatcherTimer _statusBarRefreshTimer;
        /// <summary>
        /// Field for currently show torrents.
        /// </summary>
        private TorrentStatus[] _currentFilter;
        /// <summary>
        /// Field for currently selected torrents.
        /// </summary>
        private IEnumerable<TorrentData> _selectedTorrents;

        /// <summary>
        /// Collection property of all currenly open torrents 
        /// </summary>
        public ObservableCollection<TorrentData> TorrentCollection { get; private set; }
        /// <summary>
        /// Collection property of all currenly open torrents 
        /// </summary>
        public ObservableCollection<TorrentData> FilteredCollection { get; private set; } // TODO: Has to be private? Even field?

        public Statistics StatusBarStatistics { get; private set; }

        private TorrentClient _client;

        public MainWindow()
        {
            _client = new TorrentClient();
            
            TorrentCollection = new ObservableCollection<TorrentData>();
            FilteredCollection = new ObservableCollection<TorrentData>();
            StatusBarStatistics = new Statistics();
            InitializeComponent();
            _currentFilter = new[] { TorrentStatus.Downloading, TorrentStatus.Seeding, TorrentStatus.Stopped, TorrentStatus.Hashing };
            TorrentCollection.CollectionChanged += OnTorrentCollectionChanged;
            addButton.Click += OnAddButtonClick;

            removeButton.Click += OnRemoveButtonClick;
            startButton.Click += OnStartButtonClick;
            pauseButton.Click += OnPauseButtonClick;
            stopButton.Click += OnStopButtonClick;
            downButton.Click += OnDownButtonClick;
            upButton.Click += OnUpButtonClick;

            listView.SelectionChanged += OnListViewSelectionChanged;

            listBox.SelectionChanged += OnListBoxSelectionChanged;

            _statusBarRefreshTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            _statusBarRefreshTimer.Tick += OnStatusBarRefreshTimerTick;
            _statusBarRefreshTimer.Start();

            #region TESTING
            SetTestData();
            #endregion
        }

        void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
                                  {
                                      Filter = "Torrent Files (*.torrent)|*.torrent"
                                  };

            fileDialog.ShowDialog();

            if (String.IsNullOrEmpty(fileDialog.FileName)) return;

            // TODO: torrent file add logic here
        }

        void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedTorrents != null)
            {
                foreach (var selectedTorrent in _selectedTorrents)
                {
                    _client.RemoveTorrent(selectedTorrent.Hash);
                }
            }
            else
            {
                // TODO: (Disable buttons when no selection instead)=>done
                // TODO: Should this check remain just in case?
                MessageBox.Show("No torrent selected", "Nothing selected");
            }
        }

        void OnUpButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedTorrents != null)
            {
                foreach (var selectedTorrent in _selectedTorrents)
                {
                    // TODO: Up button action for each selected torrent
                    ((TorrentData)selectedTorrent).Priority++;
                }
            }
            else
            {
                // TODO: (Disable buttons when no selection instead)=>done
                // TODO: Should this check remain just in case?
                MessageBox.Show("No torrent selected", "Nothing selected");
            }
        }

        void OnDownButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedTorrents != null)
            {
                foreach (var selectedTorrent in _selectedTorrents)
                {
                    // TODO: Down button action for each selected torrent
                    ((TorrentData)selectedTorrent).Priority--;
                }
            }
            else
            {
                // TODO: (Disable buttons when no selection instead)=>done
                // TODO: Should this check remain just in case?
                MessageBox.Show("No torrent selected", "Nothing selected");
            }
        }

        void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedTorrents != null)
            {
                foreach (var selectedTorrent in _selectedTorrents)
                {
                    _client.StopTorrent(selectedTorrent.Hash);
                }
            }
            else
            {
                // TODO: (Disable buttons when no selection instead)=>done
                // TODO: Should this check remain just in case?
                MessageBox.Show("No torrent selected", "Nothing selected");
            }
        }

        void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedTorrents != null)
            {
                foreach (var selectedTorrent in _selectedTorrents)
                {
                    _client.PauseTorrent(selectedTorrent.Hash);
                }
            }
            else
            {
                // TODO: (Disable buttons when no selection instead)=>done
                // TODO: Should this check remain just in case?
                MessageBox.Show("No torrent selected", "Nothing selected");
            }
        }

        void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                _selectedTorrents = listView.SelectedItems.Cast<TorrentData>();
                startButton.IsEnabled = true;
                pauseButton.IsEnabled = true;
                stopButton.IsEnabled = true;
                downButton.IsEnabled = true;
                upButton.IsEnabled = true;
                removeButton.IsEnabled = true;
            }
            else
            {
                _selectedTorrents = null;
                startButton.IsEnabled = false;
                pauseButton.IsEnabled = false;
                stopButton.IsEnabled = false;
                downButton.IsEnabled = false;
                upButton.IsEnabled = false;
                removeButton.IsEnabled = false;
            }

        }

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            if (_selectedTorrents != null)
            {
                foreach (var selectedTorrent in _selectedTorrents)
                {
                    _client.StartTorrent(selectedTorrent.Hash);
                }
            }
            else
            {
                // TODO: (Disable buttons when no selection instead)=>done
                // TODO: Should this check remain just in case?
                MessageBox.Show("No torrent selected", "Nothing selected");
            }
        }

        #region FOR TESTING ONLY
        private void SetTestData()
        {
            //TorrentCollection.Add(new TorrentData { TorrentName = "HashingTest", Status = TorrentStatus.Hashing });
            //TorrentCollection.Add(new TorrentData { TorrentName = "DownloadingTest", Status = TorrentStatus.Downloading });
            //TorrentCollection.Add(new TorrentData { TorrentName = "StoppedTest", Status = TorrentStatus.Stopped });
            //TorrentCollection.Add(new TorrentData { TorrentName = "SeedingTest", Status = TorrentStatus.Seeding });
        }
        #endregion

        private void OnStatusBarRefreshTimerTick(object sender, EventArgs e)
        {
            // TODO: Update statistics here
        }

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
                        _currentFilter = new[] { TorrentStatus.Seeding, TorrentStatus.Downloading };
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
