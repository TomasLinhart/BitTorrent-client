namespace BitTorrent_client
{
    public enum TorrentStatus
    {
        Stopped,
        Paused,
        Downloading,
        Seeding,
        Hashing,
        Stopping,
        Error,
        Metadata,
    }
}