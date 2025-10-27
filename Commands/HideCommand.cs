using System;
using Exiled.API.Features;
using PlayerRoles;
using CommandSystem;
using MEC;
using Shiv.Utilities;
using Shiv.Data;
using RemoteAdmin;
using System.Collections.Generic;

namespace Shiv.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class HideCommand : ICommand
    {
        public string Command => "hide";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Hide a shiv you're currently holding";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command can only be used by players.";
                return false;
            }

            var player = Player.Get(playerSender.ReferenceHub);
            if (player == null)
            {
                response = "Player not found.";
                return false;
            }

            var plugin = Plugin.Instance;
            if (plugin == null)
            {
                response = "Plugin not found.";
                return false;
            }

            if (!plugin.Config.IsEnabled)
            {
                response = "Shiv system is disabled.";
                return false;
            }

            // Check if player is a human role (not SCP)
            if (player.Role.Team == Team.SCPs)
            {
                response = "SCPs cannot hide shivs.";
                return false;
            }

            if (player.IsCuffed)
            {
                response = "Cannot hide shiv while cuffed.";
                return false;
            }

            if (CooldownManager.IsPlayerOnCooldown(player, "hide"))
            {
                var remaining = CooldownManager.GetRemainingCooldown(player, "hide");
                response = $"You must wait {remaining:F1} seconds before hiding again!";
                
                Timing.RunCoroutine(ShowCooldownReminder(player, "hide", remaining));
                return false;
            }

            if (plugin._hiddenShivs.ContainsKey(player.UserId))
            {
                response = "You already have a shiv hidden.";
                return false;
            }

            var currentItem = player.CurrentItem;
            if (currentItem == null || !ShivItemTracker.IsShivCreatedItem(currentItem.Serial))
            {
                response = "Hold a shiv to hide it.";
                return false;
            }

            var hiddenData = new HiddenShivData
            {
                ItemSerial = currentItem.Serial,
                CreatorId = player.UserId,
                CreatorName = player.Nickname,
                HiddenTime = DateTime.Now,
                ItemType = currentItem.Type
            };

            plugin._hiddenShivs[player.UserId] = hiddenData;
            player.RemoveItem(currentItem);
            CooldownManager.SetPlayerCooldown(player, "hide");

            Timing.RunCoroutine(DiscordWebhookLogger.LogHideUnhideEventCoroutine(player, "hidden", true));

            response = "Shiv hidden. Use .unhide to retrieve it.";
            return true;
        }
        
        private IEnumerator<float> ShowCooldownReminder(Player player, string commandType, float initialRemaining)
        {
            float remaining = initialRemaining;
            
            while (remaining > 0 && player != null && player.IsConnected)
            {
                yield return Timing.WaitForSeconds(1f);
                remaining -= 1f;
            }
        }
    }
}
