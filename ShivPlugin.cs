using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq;
using PlayerRoles;
using Shiv.Config;
using CommandSystem;
using RemoteAdmin;
using UnityEngine;
using MEC;
using Newtonsoft.Json;
using Shiv.Utilities;
using Shiv.Data;
using Shiv.Commands;

namespace Shiv
{
    public class Plugin : Plugin<Shiv.Config.Config>
    {
        public override string Name => "Shiv";
        public override string Author => "Ducstii";
        public override Version Version => new Version(1, 0, 0);
        public override string Prefix => "Shiv";
        
        public static Plugin? Instance { get; private set; }
        
         
        public Dictionary<string, DateTime> _playerCooldowns = new();
        public Dictionary<string, DateTime> _hideCooldowns = new();
        public Dictionary<string, DateTime> _unhideCooldowns = new();
        public Dictionary<string, DateTime> _searchCooldowns = new();
        public Dictionary<string, DateTime> _attackCooldowns = new();
        
         
        public Dictionary<string, HiddenShivData> _hiddenShivs = new();
        public Dictionary<string, SearchData> _activeSearches = new();
        public Dictionary<string, CoroutineHandle> _shivHintCoroutines = new();
        
        public override void OnEnabled()
        {
            Instance = this;
            
             
            ValidateConfig();
            
            Exiled.Events.Handlers.Player.UsingItem += OnUsingItem;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            Timing.RunCoroutine(CleanupExpiredCooldowns());
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.UsingItem -= OnUsingItem;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            _playerCooldowns.Clear();
            _hideCooldowns.Clear();
            _unhideCooldowns.Clear();
            _searchCooldowns.Clear();
            _attackCooldowns.Clear();
            _hiddenShivs.Clear();
            _activeSearches.Clear();
            _shivHintCoroutines.Clear();
            ShivItemTracker.ClearAllTrackedItems();
            Instance = null;
            base.OnDisabled();
        }
        
        private void ValidateConfig()
        {
             
            Config.WallDetectionDistance = Math.Max(0.1f, Math.Min(10f, Config.WallDetectionDistance));
            Config.SuccessChance = Math.Max(1, Math.Min(100, Config.SuccessChance));
            Config.CooldownTime = Math.Max(0f, Math.Min(300f, Config.CooldownTime));
            Config.ShivDamage = Math.Max(0f, Math.Min(1000f, Config.ShivDamage));
            Config.HpDrainRate = Math.Max(0f, Math.Min(100f, Config.HpDrainRate));
            Config.HpDrainDuration = Math.Max(0f, Math.Min(60f, Config.HpDrainDuration));
            Config.ShivAttackRange = Math.Max(0.1f, Math.Min(20f, Config.ShivAttackRange));
            Config.SearchDetectionChance = Math.Max(0f, Math.Min(1f, Config.SearchDetectionChance));
            Config.HideCooldown = Math.Max(0f, Math.Min(300f, Config.HideCooldown));
            Config.UnhideCooldown = Math.Max(0f, Math.Min(300f, Config.UnhideCooldown));
            Config.SearchCooldown = Math.Max(0f, Math.Min(300f, Config.SearchCooldown));
            Config.ShivCraftingFailureDamage = Math.Max(0f, Math.Min(1000f, Config.ShivCraftingFailureDamage));
            Config.AttackCooldown = Math.Max(0f, Math.Min(60f, Config.AttackCooldown));
            Config.MaxRayDirections = Math.Max(1, Math.Min(10, Config.MaxRayDirections));
            Config.VerticalSurfaceThreshold = Math.Max(0f, Math.Min(1f, Config.VerticalSurfaceThreshold));
            
            if (Config.Debug)
            {
                Log.Info("Config validation completed - values clamped to safe ranges");
            }
        }
        
        private void OnUsingItem(Exiled.Events.EventArgs.Player.UsingItemEventArgs ev)
        {
            if (!Config.IsEnabled) return;
            
            if (ShivItemTracker.IsShivCreatedItem(ev.Item.Serial))
            {
                ev.IsAllowed = false; 
                ShivFunctionality.HandleShivUsage(ev.Player, ev.Item);
            }
        }
        
        private void OnPlayerDied(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            if (_hiddenShivs.ContainsKey(ev.Player.UserId))
            {
                _hiddenShivs.Remove(ev.Player.UserId);
                
                if (Config.Debug)
                {
                    Log.Info($"Cleaned up hidden shiv for dead player: {ev.Player.Nickname}");
                }
            }
        }
        
        private void OnPlayerLeft(Exiled.Events.EventArgs.Player.LeftEventArgs ev)
        {
            _playerCooldowns.Remove(ev.Player.UserId);
            _hideCooldowns.Remove(ev.Player.UserId);
            _unhideCooldowns.Remove(ev.Player.UserId);
            _searchCooldowns.Remove(ev.Player.UserId);
            _attackCooldowns.Remove(ev.Player.UserId);
            _hiddenShivs.Remove(ev.Player.UserId);
            _activeSearches.Remove(ev.Player.UserId);
            
            if (_shivHintCoroutines.ContainsKey(ev.Player.UserId))
            {
                Timing.KillCoroutines(_shivHintCoroutines[ev.Player.UserId]);
                _shivHintCoroutines.Remove(ev.Player.UserId);
            }
            
            if (Config.Debug)
            {
                Log.Info($"Cleaned up all data for disconnected player: {ev.Player.Nickname}");
            }
        }
        
        private IEnumerator<float> CleanupExpiredCooldowns()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(60f); 
                
                var now = DateTime.Now;
                int totalCleaned = 0;
                
                 
                var cooldownDictionaries = new Dictionary<string, Dictionary<string, DateTime>>
                {
                    { "player", _playerCooldowns },
                    { "hide", _hideCooldowns },
                    { "unhide", _unhideCooldowns },
                    { "search", _searchCooldowns },
                    { "attack", _attackCooldowns }
                };
                
                foreach (var dict in cooldownDictionaries.Values)
                {
                    var expiredKeys = dict.Where(kvp => now > kvp.Value).Select(kvp => kvp.Key).ToList();
                    foreach (var key in expiredKeys)
                    {
                        dict.Remove(key);
                        totalCleaned++;
                    }
                }
                
                 
                var expiredSearches = _activeSearches.Where(kvp => (now - kvp.Value.StartTime).TotalSeconds > 10.0f)
                                                    .Select(kvp => kvp.Key).ToList();
                foreach (var searcherId in expiredSearches)
                {
                    _activeSearches.Remove(searcherId);
                    totalCleaned++;
                }
                
                 
                var disconnectedPlayers = _shivHintCoroutines.Where(kvp => 
                {
                    var player = Player.Get(kvp.Key);
                    return player == null || !player.IsConnected;
                }).Select(kvp => kvp.Key).ToList();
                
                foreach (var playerId in disconnectedPlayers)
                {
                    Timing.KillCoroutines(_shivHintCoroutines[playerId]);
                    _shivHintCoroutines.Remove(playerId);
                    totalCleaned++;
                }
                
                if (Config.Debug && totalCleaned > 0)
                {
                    Log.Info($"Cleaned up {totalCleaned} expired entries");
                }
            }
        }
    }
}
