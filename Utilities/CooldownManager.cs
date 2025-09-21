using System;
using System.Collections.Generic;
using Exiled.API.Features;

namespace Shiv.Utilities
{
    public static class CooldownManager
    {
        public static bool IsPlayerOnCooldown(Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return false;
            if (!Plugin.Instance._playerCooldowns.ContainsKey(player.UserId)) return false;
            
            return DateTime.Now < Plugin.Instance._playerCooldowns[player.UserId];
        }
        
        public static float GetRemainingCooldown(Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return 0f;
            if (!Plugin.Instance._playerCooldowns.ContainsKey(player.UserId)) return 0f;
            
            var remaining = Plugin.Instance._playerCooldowns[player.UserId] - DateTime.Now;
            return (float)remaining.TotalSeconds;
        }
        
        public static void SetPlayerCooldown(Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return;
            Plugin.Instance._playerCooldowns[player.UserId] = DateTime.Now.AddSeconds(Plugin.Instance.Config.CooldownTime);
        }
        
        public static bool IsPlayerOnCooldown(Player player, string commandType)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return false;
            
            Dictionary<string, DateTime> cooldownDict = commandType switch
            {
                "hide" => Plugin.Instance._hideCooldowns,
                "unhide" => Plugin.Instance._unhideCooldowns,
                "search" => Plugin.Instance._searchCooldowns,
                "attack" => Plugin.Instance._attackCooldowns,
                _ => Plugin.Instance._playerCooldowns
            };
            
            if (!cooldownDict.ContainsKey(player.UserId)) return false;
            return DateTime.Now < cooldownDict[player.UserId];
        }
        
        public static float GetRemainingCooldown(Player player, string commandType)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return 0f;
            
            Dictionary<string, DateTime> cooldownDict = commandType switch
            {
                "hide" => Plugin.Instance._hideCooldowns,
                "unhide" => Plugin.Instance._unhideCooldowns,
                "search" => Plugin.Instance._searchCooldowns,
                "attack" => Plugin.Instance._attackCooldowns,
                _ => Plugin.Instance._playerCooldowns
            };
            
            if (!cooldownDict.ContainsKey(player.UserId)) return 0f;
            var remaining = cooldownDict[player.UserId] - DateTime.Now;
            return (float)remaining.TotalSeconds;
        }
        
        public static void SetPlayerCooldown(Player player, string commandType)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return;
            
            float cooldownTime = commandType switch
            {
                "hide" => Plugin.Instance.Config.HideCooldown,
                "unhide" => Plugin.Instance.Config.UnhideCooldown,
                "search" => Plugin.Instance.Config.SearchCooldown,
                "attack" => Plugin.Instance.Config.AttackCooldown,
                _ => Plugin.Instance.Config.CooldownTime
            };
            
            Dictionary<string, DateTime> cooldownDict = commandType switch
            {
                "hide" => Plugin.Instance._hideCooldowns,
                "unhide" => Plugin.Instance._unhideCooldowns,
                "search" => Plugin.Instance._searchCooldowns,
                "attack" => Plugin.Instance._attackCooldowns,
                _ => Plugin.Instance._playerCooldowns
            };
            
            cooldownDict[player.UserId] = DateTime.Now.AddSeconds(cooldownTime);
        }
    }
}
