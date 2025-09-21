using System;

namespace Shiv.Data
{
    [Serializable]
    public class SearchData
    {
        public string SearcherId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public float Duration { get; set; }
        public bool IsCompleted { get; set; }
    }
}
