using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Client.Encryption;
using MonoTorrent.Common;

namespace BitTorrent_client
{
    public class TorrentClient
    {
        private ClientEngine _engine;

        public int TotalUploadSpeed
        {
            get { return _engine.TotalUploadSpeed; }
        }

        public int DiskReadRate
        {
            get { return _engine.DiskManager.ReadRate; }
        }

        public int DiskWriteRate
        {
            get { return _engine.DiskManager.WriteRate; }
        }

        public long TotalRead
        {
            get { return _engine.DiskManager.TotalRead; }
        }

        public long TotalWritten
        {
            get { return _engine.DiskManager.TotalWritten; }
        }

        public int OpenConnections
        {
            get { return _engine.ConnectionManager.OpenConnections; }
        }

        public int Port { get; set; }
        public string DownloadPath { get; set; }
        public string TorrentPath { get; set; }
        public string FastResumeFile { get; set; }

        private IList<TorrentManager> _torrents;
        private ObservableCollection<TorrentData> _torrentData;

        public TorrentClient(ObservableCollection<TorrentData> torrentDatas)
        {
            _torrentData = torrentDatas;
            _torrents = new List<TorrentManager>();
            Port = 80;
            DownloadPath = Path.Combine(Environment.CurrentDirectory, "Downloads");
            TorrentPath = Path.Combine(Environment.CurrentDirectory, "Torrents");
            FastResumeFile = Path.Combine(TorrentPath, "fastresume.data");

            if (!Directory.Exists(DownloadPath))
                Directory.CreateDirectory(DownloadPath);

            StartEngine();
        }

        /// <summary>
        /// Start listening on specific port
        /// </summary>
        private void StartEngine()
        {
            var engineSettings = new EngineSettings(DownloadPath, Port);
            engineSettings.PreferEncryption = false;
            engineSettings.AllowedEncryption = EncryptionTypes.All;

            _engine = new ClientEngine(engineSettings);
            _engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, Port));

            foreach (string file in Directory.GetFiles(TorrentPath))
            {
                AddTorrent(file);
            }
        }

        /// <summary>
        /// Gets default settings for torrent
        /// </summary>
        /// <returns></returns>
        private TorrentSettings GetDefaultTorrentSettings()
        {
            // 4 Upload slots - a good ratio is one slot per 5kB of upload speed
            // 50 open connections - should never really need to be changed
            // Unlimited download speed - valid range from 0 -> int.Max
            // Unlimited upload speed - valid range from 0 -> int.Max
            var torrentDefaults = new TorrentSettings(4, 150, 0, 0);
            return torrentDefaults;
        }

        /// <summary>
        /// Gets a fast resume file
        /// </summary>
        /// <returns></returns>
        private BEncodedDictionary GetFastResume()
        {
            try
            {
                return BEncodedValue.Decode<BEncodedDictionary>(File.ReadAllBytes(FastResumeFile));
            }
            catch
            {
                return new BEncodedDictionary();
            }
        }

        /// <summary>
        /// Adds a torrent file to a client
        /// </summary>
        /// <param name="file">Path to torrent</param>
        public void AddTorrent(string file)
        {
            Torrent torrent = Torrent.Load(file);

            if (_engine.Contains(torrent.InfoHash))
                return;

            File.Copy(file, Path.Combine(TorrentPath, file));

            TorrentManager manager = new TorrentManager(torrent, DownloadPath, GetDefaultTorrentSettings());

            var fastResume = GetFastResume();

            if (fastResume.ContainsKey(torrent.InfoHash.ToHex()))
                manager.LoadFastResume(
                    new FastResume((BEncodedDictionary)fastResume[torrent.InfoHash.ToHex()]));
            
            _engine.Register(manager);

            _torrents.Add(manager);
            _torrentData.Add(GetTorrentData(manager));

            manager.Start();
        }

        /// <summary>
        /// Finds a TorrentManager with specific TorrentHash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private TorrentManager FindTorrentByHash(TorrentHash hash)
        {
            return (from t in _torrents
                    where t.InfoHash.ToHex() == hash.ToString()
                    select t).SingleOrDefault();
        }

        /// <summary>
        /// Removes a torrent from a client with specific hash
        /// </summary>
        /// <param name="hash"></param>
        public void RemoveTorrent(TorrentHash hash)
        {
            var torrentManager = FindTorrentByHash(hash);

            if (torrentManager != null)
            {
                TorrentData data = (from d in _torrentData
                                   where d.Hash.ToString() == torrentManager.InfoHash.ToHex()
                                   select d).SingleOrDefault();
                _torrentData.Remove(data);
                _engine.Unregister(torrentManager);
                _torrents.Remove(torrentManager);
                File.Delete(Path.Combine(TorrentPath, torrentManager.Torrent.TorrentPath));
            }
        }

        /// <summary>
        /// Pauses a torrent with specific hash
        /// </summary>
        /// <param name="hash"></param>
        public void PauseTorrent(TorrentHash hash)
        {
            var torrentManager = FindTorrentByHash(hash);
            if (torrentManager != null)
                torrentManager.Pause();
        }

        /// <summary>
        /// Stops a torrent with specific hash
        /// </summary>
        /// <param name="hash"></param>
        public void StopTorrent(TorrentHash hash)
        {
            var torrentManager = FindTorrentByHash(hash);
            if (torrentManager != null)
                torrentManager.Stop();
        }

        /// <summary>
        /// Starts a torrent with specific hash
        /// </summary>
        /// <param name="hash"></param>
        public void StartTorrent(TorrentHash hash)
        {
            var torrentManager = FindTorrentByHash(hash);
            if (torrentManager != null)
                torrentManager.Start();
        }


        /// <summary>
        /// Returns a collection of TorrentData
        /// </summary>
        /// <returns></returns>
        public TorrentData GetTorrentData(TorrentManager torrent)
        {
            return new TorrentData()
                {
                    DownloadSpeed = torrent.Monitor.DownloadSpeed,
                    Hash = new TorrentHash(torrent.InfoHash.ToHex()),
                    Peers = torrent.Peers.Leechs,
                    Progress = torrent.Progress,
                    Seeds = torrent.Peers.Seeds,
                    Size = torrent.Torrent.Size,
                    Status = (TorrentStatus) torrent.State,
                    TorrentName = torrent.Torrent.Name,
                    UploadSpeed = torrent.Monitor.UploadSpeed
                };
        }

        public void UpdateTorrentData()
        {
            /*var torrents = from manager in _torrents 
                           from data in _torrentData
                           where */

            foreach (var torrent in _torrents)
            {
                foreach (var data in (from data in _torrentData
                                      where torrent.InfoHash.ToHex() == data.Hash.ToString()
                                     select data))
                {
                    data.DownloadSpeed = torrent.Monitor.DownloadSpeed;
                    data.Hash = new TorrentHash(torrent.InfoHash.ToHex());
                    data.Peers = torrent.Peers.Leechs;
                    data.Progress = torrent.Progress;
                    data.Seeds = torrent.Peers.Seeds;
                    data.Size = torrent.Torrent.Size;
                    data.Status = (TorrentStatus)torrent.State;
                    data.TorrentName = torrent.Torrent.Name;
                    data.UploadSpeed = torrent.Monitor.UploadSpeed;
                }
            }
        }


        /// <summary>
        /// Shutdowns torrent client and saves unfinished torrents to resume file
        /// This method must be called when Exit button is pressed!
        /// </summary>
        public void Shutdown()
        {
            var fastResume = new BEncodedDictionary();

            foreach (var torrent in _torrents)
            {
                torrent.Stop();

                while (torrent.State != TorrentState.Stopped)
                {
                    Thread.Sleep(250);
                }

                fastResume.Add(torrent.Torrent.InfoHash.ToHex(), torrent.SaveFastResume().Encode());
            }

            File.WriteAllBytes(FastResumeFile, fastResume.Encode());
            _engine.Dispose();

            Thread.Sleep(2000);
        }
    }
}
