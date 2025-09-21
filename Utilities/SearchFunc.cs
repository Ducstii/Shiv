using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using Shiv.Data;
using UnityEngine;

namespace Shiv.Utilities
{
    public static class SearchFunctionality
    {
        public static IEnumerator<float> PerformSearch(Player searcher, Player target)
        {
            string searcherId = searcher.UserId;
            string targetId = target.UserId;
            
            
            var searchData = new SearchData
            {
                SearcherId = searcherId,
                TargetId = targetId,
                StartTime = DateTime.Now,
                Duration = 3.0f,
                IsCompleted = false
            };
            
            Plugin.Instance._activeSearches[searcherId] = searchData;
            
            
            searcher.ShowHint($"Searching {target.Nickname}...", 3.5f);
            target.ShowHint($"Being searched by {searcher.Nickname}...", 3.5f);
            
            float elapsed = 0f;
            float checkInterval = 0.1f; 
            bool[] progressShown = { false, false, false }; 
            
            while (elapsed < 3.0f && !searchData.IsCompleted)
            {
                yield return Timing.WaitForSeconds(checkInterval);
                elapsed += checkInterval;
                
                
                if (searcher == null || !searcher.IsAlive || !searcher.IsConnected ||
                    target == null || !target.IsAlive || !target.IsConnected)
                {
                    if (searcher != null && searcher.IsAlive)
                    {
                        searcher.ShowHint("Search interrupted - target moved away or disconnected.", 3f);
                    }
                    Plugin.Instance._activeSearches.Remove(searcherId);
                    yield break;
                }
                
                
                float distance = Vector3.Distance(searcher.Position, target.Position);
                if (distance > 3.5f)
                {
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Search interrupted: {searcher.Nickname} and {target.Nickname} are {distance:F2} units apart (max: 3.5)");
                    }
                    searcher.ShowHint("Search failed - target moved too far away.", 3f);
                    target.ShowHint("Search interrupted - you moved too far away.", 3f);
                    Plugin.Instance._activeSearches.Remove(searcherId);
                    yield break;
                }
                
                
                int progress = (int)((elapsed / 3.0f) * 100);
                if (Plugin.Instance.Config.Debug && progress % 10 == 0)
                {
                    Log.Info($"Search progress: {progress}% (elapsed: {elapsed:F1}s, distance: {distance:F2})");
                }
                
                if (progress >= 25 && !progressShown[0])
                {
                    searcher.ShowHint($"Searching {target.Nickname}... 25%", 1f);
                    progressShown[0] = true;
                }
                else if (progress >= 50 && !progressShown[1])
                {
                    searcher.ShowHint($"Searching {target.Nickname}... 50%", 1f);
                    progressShown[1] = true;
                }
                else if (progress >= 75 && !progressShown[2]) 
                {
                    searcher.ShowHint($"Searching {target.Nickname}... 75%", 1f);
                    progressShown[2] = true;
                }
            }
            
            
            searchData.IsCompleted = true;
            Plugin.Instance._activeSearches.Remove(searcherId);
            
            if (Plugin.Instance.Config.Debug)
            {
                Log.Info($"Search completed: {searcher.Nickname} finished searching {target.Nickname}");
            }
            
            
            if (Plugin.Instance._hiddenShivs.ContainsKey(targetId))
            {
                  
                if (UnityEngine.Random.Range(0f, 1f) < Plugin.Instance.Config.SearchDetectionChance)
                {
                    var hiddenData = Plugin.Instance._hiddenShivs[targetId];
                    Plugin.Instance._hiddenShivs.Remove(targetId);
                    
                    
                    var confiscatedItem = searcher.AddItem(hiddenData.ItemType);
                    if (confiscatedItem != null)
                    {
                        ShivItemTracker.RegisterShivItem(confiscatedItem.Serial, searcher, hiddenData.ItemType);
                        
                        
                        searcher.ShowHint($"Found and confiscated a hidden shiv from {target.Nickname}!", 5f);
                        target.ShowHint($"Your hidden shiv was found and confiscated by {searcher.Nickname}!", 5f);
                        
                        Timing.RunCoroutine(DiscordWebhookLogger.LogSearchEventCoroutine(searcher, target, true, true));
                        
                        if (Plugin.Instance.Config.Debug)
                        {
                            Log.Info($"Search successful: {searcher.Nickname} found shiv on {target.Nickname} - Shiv given to searcher");
                        }
                    }
                }
                else
                {
                     
                    searcher.ShowHint($"No shiv found on {target.Nickname}.", 3f);
                    target.ShowHint($"Search completed - your shiv remains hidden.", 3f);
                    
                    Timing.RunCoroutine(DiscordWebhookLogger.LogSearchEventCoroutine(searcher, target, true, false));
                    
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Search failed: {searcher.Nickname} failed to detect shiv on {target.Nickname}");
                    }
                }
            }
            else
            {
                 
                searcher.ShowHint($"No shiv found on {target.Nickname}.", 3f);
                target.ShowHint($"Search completed - nothing found.", 3f);
                
                Timing.RunCoroutine(DiscordWebhookLogger.LogSearchEventCoroutine(searcher, target, false, false));
                
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Search completed: {searcher.Nickname} found no shiv on {target.Nickname}");
                }
            }
        }
    }
}
