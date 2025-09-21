using System;
using Exiled.API.Features;
using PlayerRoles;
using CommandSystem;
using MEC;
using Shiv.Utilities;
using RemoteAdmin;

namespace Shiv.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class UnhideCommand : ICommand
    {
        public string Command => "unhide";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Bring out your hidden shiv";

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
                response = "SCPs cannot unhide shivs.";
                return false;
            }

            if (CooldownManager.IsPlayerOnCooldown(player, "unhide"))
            {
                response = $"You must wait {CooldownManager.GetRemainingCooldown(player, "unhide"):F1} seconds before unhiding again!";
                return false;
            }

            if (!plugin._hiddenShivs.ContainsKey(player.UserId))
            {
                response = "No shiv hidden.";
                return false;
            }

            if (player.IsCuffed)
            {
                response = "Cannot unhide while cuffed.";
                return false;
            }

            var hiddenData = plugin._hiddenShivs[player.UserId];
            var item = player.AddItem(hiddenData.ItemType);
            
            if (item != null)
            {
                ShivItemTracker.RegisterShivItem(item.Serial, player, hiddenData.ItemType);
                plugin._hiddenShivs.Remove(player.UserId);
                CooldownManager.SetPlayerCooldown(player, "unhide");
                
                if (!plugin._shivHintCoroutines.ContainsKey(player.UserId))
                {
                    var coroutineHandle = Timing.RunCoroutine(ShivFunctionality.ShowShivHint(player));
                    plugin._shivHintCoroutines[player.UserId] = coroutineHandle;
                }
                
                Timing.RunCoroutine(DiscordWebhookLogger.LogHideUnhideEventCoroutine(player, "unhidden", true));
                
                response = "Shiv retrieved.";
                return true;
            }
            else
            {
                response = "Failed to retrieve shiv.";
                return false;
            }
        }
    }
}
