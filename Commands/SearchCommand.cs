using System;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using CommandSystem;
using MEC;
using Shiv.Utilities;
using RemoteAdmin;

namespace Shiv.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class SearchCommand : ICommand
    {
        public string Command => "search";
        public string[] Aliases => new string[] { "frisk" };
        public string Description => "Search the player you're looking at for hidden shivs (MTF and Guards only)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command can only be used by players.";
                return false;
            }

            var searcher = Player.Get(playerSender.ReferenceHub);
            if (searcher == null)
            {
                response = "Player not found.";
                return false;
            }

            if (searcher.IsCuffed)
            {
                response = "Cannot search while cuffed.";
                return false;
            }

            if (searcher.Role != PlayerRoles.RoleTypeId.FacilityGuard && 
                searcher.Role != PlayerRoles.RoleTypeId.NtfPrivate && 
                searcher.Role != PlayerRoles.RoleTypeId.NtfSergeant && 
                searcher.Role != PlayerRoles.RoleTypeId.NtfCaptain && 
                searcher.Role != PlayerRoles.RoleTypeId.NtfSpecialist)
            {
                if (Plugin.Instance?.Config.Debug == true)
                {
                    Log.Info($"Player {searcher.Nickname} (Role: {searcher.Role}) attempted to use search command but lacks permission");
                }
                response = "Only MTF and Guards can search.";
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

            if (CooldownManager.IsPlayerOnCooldown(searcher, "search"))
            {
                response = $"You must wait {CooldownManager.GetRemainingCooldown(searcher, "search"):F1} seconds before searching again!";
                return false;
            }

            if (plugin._activeSearches.ContainsKey(searcher.UserId))
            {
                response = "You are already searching someone.";
                return false;
            }

            bool originalDebug = plugin.Config.Debug;
            plugin.Config.Debug = true;
            
            Player? targetPlayer = PlayerDetection.FindPlayerInCone(searcher, 3.0f, 60f);
            
            plugin.Config.Debug = originalDebug;

            if (targetPlayer == null)
            {
                response = "Look at a player to search them.";
                return false;
            }

            if (plugin._activeSearches.Values.Any(s => s.TargetId == targetPlayer.UserId))
            {
                response = $"{targetPlayer.Nickname} is already being searched.";
                return false;
            }

            CooldownManager.SetPlayerCooldown(searcher, "search");
            Timing.RunCoroutine(SearchFunctionality.PerformSearch(searcher, targetPlayer));
            
            response = $"Starting search of {targetPlayer.Nickname}...";
            return true;
        }
    }
}
