namespace Core.Common.DataModels
{
    public class ProgressReport
    {
        public int CurrentProgress { get; set; }
        public int TotalProgress { get; set; }
        public string Message { get; set; } = "Processing";
    }
}
