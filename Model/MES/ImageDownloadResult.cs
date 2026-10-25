namespace OpenEAP.Model
{
    public sealed class ImageDownloadResult
    {
        private int _totalRows;
        private int _processedCount;
        private int _expectedDownloadCount;
        private int _successCount;
        private int _failCount;
        private int _emptyImagePathCount;
        private int _skippedInvalidUrlCount;
        public int TotalRows => _totalRows;
        public int ProcessedCount => _processedCount;
        public int ExpectedDownloadCount => _expectedDownloadCount;
        public int SuccessCount => _successCount;
        public int FailCount => _failCount;
        public int EmptyImagePathCount => _emptyImagePathCount;
        public int SkippedInvalidUrlCount => _skippedInvalidUrlCount;
        public string RootFolder { get; set; } = string.Empty;
        public void AddTotalRows(int count) => Interlocked.Add(ref _totalRows, count);
        public void IncrementProcessedCount() => Interlocked.Increment(ref _processedCount);
        public void IncrementExpectedDownloadCount() => Interlocked.Increment(ref _expectedDownloadCount);
        public void IncrementSuccessCount() => Interlocked.Increment(ref _successCount);
        public void IncrementFailCount() => Interlocked.Increment(ref _failCount);
        public void IncrementEmptyImagePathCount() => Interlocked.Increment(ref _emptyImagePathCount);
        public void IncrementSkippedInvalidUrlCount() => Interlocked.Increment(ref _skippedInvalidUrlCount);
    }
}
