namespace Core.Common.DataModels
{
    public record ProgressReport
    {
        public int CurrentProgress { get; init; }
        public int TotalProgress { get; init; }
        public string Message { get; init; } = "Processing";
    }
}
