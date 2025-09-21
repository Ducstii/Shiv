using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;
using MEC;
using Shiv.Data;

namespace Shiv.Utilities
{
    public static class ShivFunctionality
    {
        public static void GiveAdrenaline(Player player)
        {
            if (player == null || !player.IsAlive || !player.IsConnected)
            {
                Log.Warn($"Cannot give shiv to invalid player: {player?.Nickname ?? "null"}");
                return;
            }

            try
            {
                var item = player.AddItem(ItemType.Adrenaline);
                
                if (item != null)
                {
                    ShivItemTracker.RegisterShivItem(item.Serial, player, ItemType.Adrenaline);
                    
                    if (!Plugin.Instance._shivHintCoroutines.ContainsKey(player.UserId))
                    {
                        var coroutineHandle = Timing.RunCoroutine(ShowShivHint(player));
                        Plugin.Instance._shivHintCoroutines[player.UserId] = coroutineHandle;
                    }
                    
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Gave shiv item with serial {item.Serial} to {player.Nickname}");
                    }
                }
                else
                {
                    Log.Warn($"Failed to create shiv item for {player.Nickname}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to give shiv to {player.Nickname}: {ex.Message}");
            }
        }
        
        public static void HandleShivUsage(Player player, Exiled.API.Features.Items.Item item)
        {
            if (player == null || item == null) return;
            if (!ShivItemTracker.IsShivCreatedItem(item.Serial)) return;
            
            if (player.Role.Team == Team.SCPs)
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Shiv attack blocked for {player.Nickname} - SCPs cannot use shivs");
                }
                return;
            }
            
            if (player.IsCuffed)
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Shiv attack blocked for {player.Nickname} - Player is cuffed");
                }
                return;
            }
            
            if (CooldownManager.IsPlayerOnCooldown(player, "attack"))
            {
                var remaining = CooldownManager.GetRemainingCooldown(player, "attack");
                
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Shiv attack blocked for {player.Nickname} - Attack cooldown: {remaining:F1}s remaining");
                }
                return;
            }
            
            Player? closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (var target in Player.List)
            {
                if (target == player || !target.IsAlive) continue;
                
                float distance = Vector3.Distance(player.Position, target.Position);
                if (distance <= Plugin.Instance.Config.ShivAttackRange && distance < closestDistance)
                {
                    Vector3 directionToTarget = (target.Position - player.Position).normalized;
                    Vector3 playerForward = player.CameraTransform?.forward ?? player.Transform.forward;
                    
                    float dot = Vector3.Dot(playerForward, directionToTarget);
                    if (dot > 0.5f)
                    {
                        closestTarget = target;
                        closestDistance = distance;
                    }
                }
            }
            
            if (closestTarget != null)
            {
                closestTarget.Hurt(Plugin.Instance.Config.ShivDamage, Exiled.API.Enums.DamageType.Scp, "Shiv Attack");
                
                Timing.RunCoroutine(ApplyHpDrain(closestTarget));
                
                bool shivConsumed = false;
                if (Plugin.Instance.Config.ConsumeShivOnAttack)
                {
                    player.RemoveItem(item);
                    ShivItemTracker.RemoveShivItem(item.Serial);
                    shivConsumed = true;
                }
                
                CooldownManager.SetPlayerCooldown(player, "attack");
                
                Timing.RunCoroutine(ShowDelayedAttackFeedback(player, closestTarget));
                Timing.RunCoroutine(DiscordWebhookLogger.LogShivAttackCoroutine(player, closestTarget, Plugin.Instance.Config.ShivDamage, shivConsumed));
                
                if (Plugin.Instance.Config.Debug)
                {
                    string consumeStatus = shivConsumed ? "Shiv consumed" : "Shiv retained";
                    Log.Info($"Shiv attack: {player.Nickname} -> {closestTarget.Nickname} ({Plugin.Instance.Config.ShivDamage} damage) - {consumeStatus}");
                }
            }
            else
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Shiv attack failed: {player.Nickname} - no valid target in range");
                }
            }
        }
        
        private static IEnumerator<float> ApplyHpDrain(Player target)
        {
            float duration = Plugin.Instance.Config.HpDrainDuration;
            float drainRate = Plugin.Instance.Config.HpDrainRate;
            float elapsed = 0f;
            
            while (elapsed < duration && target != null && target.IsAlive)
            {
                yield return Timing.WaitForSeconds(1f);
                elapsed += 1f;
                
                if (target != null && target.IsAlive)
                {
                    target.Hurt(drainRate, Exiled.API.Enums.DamageType.Scp, "Shiv Bleeding");
                    
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Applied HP drain to {target.Nickname}: -{drainRate} HP (Time: {elapsed}/{duration})");
                    }
                }
                else
                {
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"HP drain stopped for {target?.Nickname ?? "Unknown"} - Target died or disconnected");
                    }
                    yield break;
                }
            }
            
            if (target != null && target.IsAlive)
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"HP drain completed for {target.Nickname} - Wound healed naturally");
                }
            }
        }
        
        private static IEnumerator<float> ShowDelayedAttackFeedback(Player attacker, Player target)
        {
            yield return Timing.WaitForSeconds(0.2f);
            
            if (attacker != null && attacker.IsAlive)
            {
                attacker.ShowHint($"Attacked {target?.Nickname ?? "someone"} with shiv.", 3f);
            }
            
            if (target != null && target.IsAlive)
            {
                target.ShowHint($"Attacked by {attacker?.Nickname ?? "someone"} with shiv.", 3f);
            }
        }
        
        public static IEnumerator<float> ShowCraftingFailureFeedback(Player player, float damage)
        {
            yield return Timing.WaitForSeconds(0.2f);
            
            if (player != null && player.IsAlive)
            {
                player.ShowHint($"Crafting attempt failed.", 3f);
            }
        }
        
        public static IEnumerator<float> ShowShivHint(Player player)
        {
            while (player != null && player.IsAlive && player.IsConnected)
            {
                var currentItem = player.CurrentItem;
                if (currentItem != null && ShivItemTracker.IsShivCreatedItem(currentItem.Serial))
                {
                    player.ShowHint("Shiv", 1.1f);
                }
                
                yield return Timing.WaitForSeconds(0.2f);
            }
            
            if (Plugin.Instance._shivHintCoroutines.ContainsKey(player?.UserId ?? ""))
            {
                Plugin.Instance._shivHintCoroutines.Remove(player?.UserId ?? "");
            }
        }
    }
}
