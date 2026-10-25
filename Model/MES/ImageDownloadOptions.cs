namespace OpenEAP.Model
{
    public sealed class ImageDownloadOptions
    {
        public int BatchSize { get; set; } = 200;
        public int Parallelism { get; set; } = 10;
        public int MaxRetryCount { get; set; } = 2;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
        public string RootFolder { get; set; } = string.Empty;
    }
}
