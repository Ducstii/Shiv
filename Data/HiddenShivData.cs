using System;
using Exiled.API.Enums;

namespace Shiv.Data
{
    [Serializable]
    public class HiddenShivData
    {
        public uint ItemSerial { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public DateTime HiddenTime { get; set; }
        public ItemType ItemType { get; set; }
    }
}
