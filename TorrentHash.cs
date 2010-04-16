using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitTorrent_client
{
    public class TorrentHash
    {
        private string _infoHash;

        public TorrentHash(string hash)
        {
            _infoHash = hash;
        }

        public override string  ToString()
        {
            return _infoHash;
        }
    }
}
