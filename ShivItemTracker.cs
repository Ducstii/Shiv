using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Enums;

namespace Shiv
{

    public static class ShivItemTracker
    {
        private static Dictionary<uint, ShivItemData> _trackedItems = new();
        

        [Serializable]
        public class ShivItemData
        {
            public bool IsShivCreated { get; set; }
            public DateTime CreatedTime { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
            public ItemType ItemType { get; set; }
        }

        public static void RegisterShivItem(uint itemSerial, Player creator, ItemType itemType)
        {
            _trackedItems[itemSerial] = new ShivItemData
            {
                IsShivCreated = true,
                CreatedTime = DateTime.Now,
                CreatorId = creator.UserId,
                CreatorName = creator.Nickname,
                ItemType = itemType
            };
            
            if (Plugin.Instance?.Config.Debug == true)
            {
                Log.Info($"Registered shiv item: Serial {itemSerial}, Creator: {creator.Nickname}, Type: {itemType}");
            }
        }
        
        public static bool IsShivCreatedItem(uint itemSerial)
        {
            return _trackedItems.ContainsKey(itemSerial) && _trackedItems[itemSerial].IsShivCreated;
        }
        

        public static ShivItemData? GetShivItemData(uint itemSerial)
        {
            return _trackedItems.ContainsKey(itemSerial) ? _trackedItems[itemSerial] : null;
        }
        

        public static void RemoveShivItem(uint itemSerial)
        {
            if (_trackedItems.ContainsKey(itemSerial))
            {
                _trackedItems.Remove(itemSerial);
                
                if (Plugin.Instance?.Config.Debug == true)
                {
                    Log.Info($"Removed tracking for shiv item: Serial {itemSerial}");
                }
            }
        }
        

        public static Dictionary<uint, ShivItemData> GetAllTrackedItems()
        {
            return new Dictionary<uint, ShivItemData>(_trackedItems);
        }
        

        public static void ClearAllTrackedItems()
        {
            _trackedItems.Clear();
            
            if (Plugin.Instance?.Config.Debug == true)
            {
                Log.Info("Cleared all tracked shiv items");
            }
        }

        public static string GetStatistics()
        {
            var totalItems = _trackedItems.Count;
            var adrenalineItems = 0;
            var oldestItem = DateTime.MaxValue;
            var newestItem = DateTime.MinValue;
            
            foreach (var item in _trackedItems.Values)
            {
                if (item.ItemType == ItemType.Adrenaline)
                    adrenalineItems++;
                
                if (item.CreatedTime < oldestItem)
                    oldestItem = item.CreatedTime;
                
                if (item.CreatedTime > newestItem)
                    newestItem = item.CreatedTime;
            }
            
            return $"Shiv Items Statistics:\n" +
                   $"Total Items: {totalItems}\n" +
                   $"Adrenaline Items: {adrenalineItems}\n" +
                   $"Oldest Item: {(oldestItem == DateTime.MaxValue ? "None" : oldestItem.ToString("HH:mm:ss"))}\n" +
                   $"Newest Item: {(newestItem == DateTime.MinValue ? "None" : newestItem.ToString("HH:mm:ss"))}";
        }
    }
}
