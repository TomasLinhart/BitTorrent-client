using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
            TorrentCollection = new ObservableCollection<TorrentData>();
            FilteredCollection = new ObservableCollection<TorrentData>();

            _client = new TorrentClient(TorrentCollection);

            AppDomain.CurrentDomain.ProcessExit += delegate { _client.Shutdown(); };
            AppDomain.CurrentDomain.UnhandledException += 
                delegate(object sender, UnhandledExceptionEventArgs e)
                    {
                        MessageBox.Show("Exception", e.ExceptionObject.ToString()); 
                        _client.Shutdown();
                    };
            Thread.GetDomain().UnhandledException += 
                delegate(object sender, UnhandledExceptionEventArgs e)
                    {
                        MessageBox.Show("Exception", e.ExceptionObject.ToString()); 
                        _client.Shutdown();
                    };
            
            StatusBarStatistics = new Statistics();
            InitializeComponent();
            _currentFilter = new[] { TorrentStatus.Stopped, TorrentStatus.Paused, TorrentStatus.Downloading, TorrentStatus.Seeding, TorrentStatus.Hashing, TorrentStatus.Stopping, TorrentStatus.Error, TorrentStatus.Metadata };
            TorrentCollection.CollectionChanged += OnTorrentCollectionChanged;
            addButton.Click += OnAddButtonClick;

            removeButton.Click += OnRemoveButtonClick;
            startButton.Click += OnStartButtonClick;
            pauseButton.Click += OnPauseButtonClick;
            stopButton.Click += OnStopButtonClick;

            listView.SelectionChanged += OnListViewSelectionChanged;

            listBox.SelectionChanged += OnListBoxSelectionChanged;

            _statusBarRefreshTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            _statusBarRefreshTimer.Tick += OnStatusBarRefreshTimerTick;
            _statusBarRefreshTimer.Start();

            listBox.SelectedIndex = 0;
        }


        void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
                                  {
                                      Filter = "Torrent Files (*.torrent)|*.torrent"
                                  };

            fileDialog.ShowDialog();

            if (String.IsNullOrEmpty(fileDialog.FileName)) return;

            _client.AddTorrent(fileDialog.FileName);
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
                removeButton.IsEnabled = true;
            }
            else
            {
                _selectedTorrents = null;
                startButton.IsEnabled = false;
                pauseButton.IsEnabled = false;
                stopButton.IsEnabled = false;
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

        private void OnStatusBarRefreshTimerTick(object sender, EventArgs e)
        {
            _client.UpdateTorrentData();

            StatusBarStatistics.OverallDownloadSpeed = _client.TotalDownloadSpeed / 1024;
            StatusBarStatistics.OverallUploadSpeed = _client.TotalUploadSpeed / 1024;
            StatusBarStatistics.TotalDownloaded = _client.TotalWritten / 1024 / 1024;
            StatusBarStatistics.TotalUploaded = _client.TotalRead / 1024 / 1024;
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
                        _currentFilter = new[] { TorrentStatus.Stopped, TorrentStatus.Paused, TorrentStatus.Downloading, TorrentStatus.Seeding, TorrentStatus.Hashing, TorrentStatus.Stopping, TorrentStatus.Error, TorrentStatus.Metadata };
                        break;
                }
            }
            FilterCollection();
        }

        private void OnMenuItemAddClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Torrent Files (*.torrent)|*.torrent"
            };

            fileDialog.ShowDialog();

            if (String.IsNullOrEmpty(fileDialog.FileName)) return;

            _client.AddTorrent(fileDialog.FileName);
        }

        private void OnMenuItemExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
