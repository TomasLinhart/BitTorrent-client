using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitTorrent_client
{
    class Preferences
    {
        public Uri DownloadPath { get; set; }
        public int Port { get; set; }
        public Uri ResumePath { get; set; }
    }
}
