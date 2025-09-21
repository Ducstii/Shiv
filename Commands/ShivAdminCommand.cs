using System;
using Exiled.API.Features;
using CommandSystem;
using MEC;
using Shiv.Utilities;

namespace Shiv.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ShivAdminCommand : ICommand
    {
        public string Command => "shiv";
        public string[] Aliases => new string[] { "shivadm", "shivtoggle" };
        public string Description => "Command to enable/disable the shiv system";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var plugin = Plugin.Instance;
            if (plugin == null)
            {
                response = "Shiv plugin not found.";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = $"Shiv system is currently {(plugin.Config.IsEnabled ? "enabled" : "disabled")}.\n" +
                          "Usage: shiv <enable|disable|status|reset|debug <enable|disable>>";
                return false;
            }

            string action = arguments.At(0).ToLower();
            var adminPlayer = Player.Get(sender);
            
            switch (action)
            {
                case "enable":
                case "on":
                case "true":
                    plugin.Config.IsEnabled = true;
                    response = "Shiv system enabled.";
                    Log.Info($"Shiv system enabled by {sender.LogName}");
                    
                    if (adminPlayer != null)
                    {
                        Timing.RunCoroutine(DiscordWebhookLogger.LogAdminCommandCoroutine(adminPlayer, "shiv", "enable", true));
                    }
                    break;
                    
                case "disable":
                case "off":
                case "false":
                    plugin.Config.IsEnabled = false;
                    response = "Shiv system disabled.";
                    Log.Info($"Shiv system disabled by {sender.LogName}");
                    
                    if (adminPlayer != null)
                    {
                        Timing.RunCoroutine(DiscordWebhookLogger.LogAdminCommandCoroutine(adminPlayer, "shiv", "disable", true));
                    }
                    break;
                    
                case "status":
                case "info":
                    var stats = ShivItemTracker.GetStatistics();
                    response = $"Shiv System Status:\n" +
                              $"Enabled: {plugin.Config.IsEnabled}\n" +
                              $"Debug Mode: {plugin.Config.Debug}\n" +
                              $"Success Chance: 1 in {plugin.Config.SuccessChance}\n" +
                              $"Cooldown Time: {plugin.Config.CooldownTime}s\n" +
                              $"Attack Damage: {plugin.Config.ShivDamage} HP\n" +
                              $"HP Drain: {plugin.Config.HpDrainRate}/s for {plugin.Config.HpDrainDuration}s\n" +
                              $"Attack Range: {plugin.Config.ShivAttackRange}m\n\n" +
                              $"{stats}";
                    break;
                    
                case "reset":
                    plugin._playerCooldowns.Clear();
                    ShivItemTracker.ClearAllTrackedItems();
                    response = "Shiv system reset.";
                    Log.Info($"Shiv system reset by {sender.LogName}");
                    
                    if (adminPlayer != null)
                    {
                        Timing.RunCoroutine(DiscordWebhookLogger.LogAdminCommandCoroutine(adminPlayer, "shiv", "reset", true));
                    }
                    break;
                    
                case "debug":
                    if (arguments.Count < 2)
                    {
                        response = $"Debug mode is currently {(plugin.Config.Debug ? "enabled" : "disabled")}.\n" +
                                  "Usage: shiv debug <enable|disable>";
                        return false;
                    }
                    
                    string debugAction = arguments.At(1).ToLower();
                    if (debugAction == "enable" || debugAction == "on" || debugAction == "true")
                    {
                        plugin.Config.Debug = true;
                        response = "Debug enabled.";
                    }
                    else if (debugAction == "disable" || debugAction == "off" || debugAction == "false")
                    {
                        plugin.Config.Debug = false;
                        response = "Debug disabled.";
                    }
                    else
                    {
                        response = "Invalid action. Use enable/disable.";
                        return false;
                    }
                    Log.Info($"Shiv debug mode {(plugin.Config.Debug ? "enabled" : "disabled")} by {sender.LogName}");
                    
                    if (adminPlayer != null)
                    {
                        Timing.RunCoroutine(DiscordWebhookLogger.LogAdminCommandCoroutine(adminPlayer, "shiv", $"debug {debugAction}", true));
                    }
                    break;
                    
                default:
                    response = "Invalid action. Use: enable, disable, status, reset, or debug <enable|disable>";
                    return false;
            }

            return true;
        }
    }
}
