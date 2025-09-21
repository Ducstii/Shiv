using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using PlayerRoles;
using CommandSystem;
using MEC;
using Shiv.Utilities;
using RemoteAdmin;

namespace Shiv.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ShivCommand : ICommand
    {
        public string Command => "shiv";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Attempt to create a shiv by looking at a nearby wall";

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
                response = "Shiv plugin is disabled.";
                return false;
            }

            // Check if player is a human role (not SCP)
            if (player.Role.Team == Team.SCPs)
            {
                response = "SCPs cannot craft shivs.";
                return false;
            }

            if (player.IsCuffed)
            {
                response = "Cannot craft shiv while cuffed.";
                return false;
            }

            if (CooldownManager.IsPlayerOnCooldown(player))
            {
                response = $"You must wait {CooldownManager.GetRemainingCooldown(player):F1} seconds before using shiv again!";
                return false;
            }

            if (!WallDetection.IsLookingAtWall(player))
            {
                response = "Look at a wall to craft a shiv.";
                return false;
            }

            if (UnityEngine.Random.Range(1, plugin.Config.SuccessChance + 1) == 1)
            {
                ShivFunctionality.GiveAdrenaline(player);
                response = "Shiv crafted.";
                CooldownManager.SetPlayerCooldown(player);
                
                Timing.RunCoroutine(DiscordWebhookLogger.LogShivCreationCoroutine(player, true));
                
                if (plugin.Config.Debug)
                {
                    Log.Info($"Player {player.Nickname} successfully created a shiv");
                }
            }
            else
            {
                float failureDamage = plugin.Config.ShivCraftingFailureDamage;
                player.Hurt(failureDamage, Exiled.API.Enums.DamageType.Scp, "Bloody wounds on the hands");
                
                Timing.RunCoroutine(ShivFunctionality.ShowCraftingFailureFeedback(player, failureDamage));
                Timing.RunCoroutine(DiscordWebhookLogger.LogShivCreationCoroutine(player, false, failureDamage));
                
                response = "Crafting failed.";
                CooldownManager.SetPlayerCooldown(player);
                
                if (plugin.Config.Debug)
                {
                    Log.Info($"Player {player.Nickname} failed to create a shiv and took {failureDamage} damage");
                }
            }

            return true;
        }
    }
}
