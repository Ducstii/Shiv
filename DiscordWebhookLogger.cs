using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;
using MEC;

namespace Shiv
{
    public static class DiscordWebhookLogger
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        public static async Task LogShivCreation(Player player, bool success, float damage = 0f)
        {
            if (!Plugin.Instance?.Config.EnableDiscordLogging == true || 
                !Plugin.Instance?.Config.LogShivCreation == true ||
                string.IsNullOrEmpty(Plugin.Instance?.Config.DiscordWebhookUrl))
                return;

            var fields = new List<object>
            {
                new { name = "Player", value = player.Nickname, inline = true },
                new { name = "Time", value = DateTime.Now.ToString("HH:mm:ss UTC"), inline = true }
            };

            if (Plugin.Instance?.Config.Debug == true)
            {
                fields.Add(new { name = "Role", value = player.Role.ToString(), inline = true });
                fields.Add(new { name = "Location", value = $"X: {player.Position.x:F1}, Y: {player.Position.y:F1}, Z: {player.Position.z:F1}", inline = false });
            }

            if (!success)
            {
                fields.Add(new { name = "Damage Taken", value = $"{damage} HP", inline = true });
            }

            var embed = new
            {
                title = success ? "Shiv item created" : "Shiv creation failed",
                color = success ? 0x00ff00 : 0xff0000,
                fields = fields.ToArray()
            };

            await SendWebhook(embed);
        }

        public static async Task LogShivAttack(Player attacker, Player target, float damage, bool consumed)
        {
            if (!Plugin.Instance?.Config.EnableDiscordLogging == true || 
                !Plugin.Instance?.Config.LogShivAttacks == true ||
                string.IsNullOrEmpty(Plugin.Instance?.Config.DiscordWebhookUrl))
                return;

            var embed = new
            {
                title = "Shiv attack",
                color = 0xff6600,
                fields = new object[]
                {
                    new { name = "Attacker", value = attacker.Nickname, inline = true },
                    new { name = "Target", value = target.Nickname, inline = true },
                    new { name = "Damage", value = $"{damage} HP", inline = true },
                    new { name = "Time", value = DateTime.Now.ToString("HH:mm:ss UTC"), inline = true }
                }.Concat(Plugin.Instance?.Config.Debug == true ? new object[]
                {
                    new { name = "Attacker Role", value = attacker.Role.ToString(), inline = true },
                    new { name = "Target Role", value = target.Role.ToString(), inline = true },
                    new { name = "Item Status", value = consumed ? "Consumed" : "Retained", inline = true },
                    new { name = "Location", value = $"X: {attacker.Position.x:F1}, Y: {attacker.Position.y:F1}, Z: {attacker.Position.z:F1}", inline = false }
                } : new object[]
                {
                    new { name = "Item Status", value = consumed ? "Consumed" : "Retained", inline = true }
                }).ToArray()
            };

            await SendWebhook(embed);
        }

        public static async Task LogSearchEvent(Player searcher, Player target, bool foundShiv, bool confiscated)
        {
            if (!Plugin.Instance?.Config.EnableDiscordLogging == true || 
                !Plugin.Instance?.Config.LogSearchEvents == true ||
                string.IsNullOrEmpty(Plugin.Instance?.Config.DiscordWebhookUrl))
                return;

            string title, result;
            if (foundShiv)
            {
                title = confiscated ? "Item confiscated" : "Item detected";
                result = confiscated ? "Item removed" : "Item found";
            }
            else
            {
                title = "Search completed";
                result = "No items found";
            }

            var embed = new
            {
                title = title,
                color = foundShiv ? (confiscated ? 0xff0000 : 0xffaa00) : 0x00ff00,
                fields = new object[]
                {
                    new { name = "Searcher", value = searcher.Nickname, inline = true },
                    new { name = "Target", value = target.Nickname, inline = true },
                    new { name = "Result", value = result, inline = true },
                    new { name = "Time", value = DateTime.Now.ToString("HH:mm:ss UTC"), inline = true }
                }.Concat(Plugin.Instance?.Config.Debug == true ? new object[]
                {
                    new { name = "Searcher Role", value = searcher.Role.ToString(), inline = true },
                    new { name = "Target Role", value = target.Role.ToString(), inline = true },
                    new { name = "Location", value = $"X: {searcher.Position.x:F1}, Y: {searcher.Position.y:F1}, Z: {searcher.Position.z:F1}", inline = false }
                } : new object[0]).ToArray()
            };

            await SendWebhook(embed);
        }

        public static async Task LogAdminCommand(Player admin, string command, string action, bool success)
        {
            if (!Plugin.Instance?.Config.EnableDiscordLogging == true || 
                !Plugin.Instance?.Config.LogAdminCommands == true ||
                string.IsNullOrEmpty(Plugin.Instance?.Config.DiscordWebhookUrl))
                return;

            var embed = new
            {
                title = success ? "Admin command executed" : "Admin command failed",
                color = success ? 0x0099ff : 0xff0000,
                fields = new object[]
                {
                    new { name = "Admin", value = admin?.Nickname ?? "Console", inline = true },
                    new { name = "Action", value = action, inline = true },
                    new { name = "Result", value = success ? "Success" : "Failed", inline = true },
                    new { name = "Time", value = DateTime.Now.ToString("HH:mm:ss UTC"), inline = true }
                }.Concat(Plugin.Instance?.Config.Debug == true ? new object[]
                {
                    new { name = "Command", value = command, inline = true }
                } : new object[0]).ToArray()
            };

            await SendWebhook(embed);
        }

        public static async Task LogHideUnhideEvent(Player player, string action, bool success, string reason = "")
        {
            if (!Plugin.Instance?.Config.EnableDiscordLogging == true ||
                string.IsNullOrEmpty(Plugin.Instance?.Config.DiscordWebhookUrl))
                return;

            var fields = new List<object>
            {
                new { name = "Player", value = player.Nickname, inline = true },
                new { name = "Action", value = action, inline = true },
                new { name = "Result", value = success ? "Success" : "Failed", inline = true },
                new { name = "Time", value = DateTime.Now.ToString("HH:mm:ss UTC"), inline = true }
            };

            if (Plugin.Instance?.Config.Debug == true)
            {
                fields.Add(new { name = "Role", value = player.Role.ToString(), inline = true });
                fields.Add(new { name = "Location", value = $"X: {player.Position.x:F1}, Y: {player.Position.y:F1}, Z: {player.Position.z:F1}", inline = false });
            }

            if (!success && !string.IsNullOrEmpty(reason))
            {
                fields.Add(new { name = "Reason", value = reason, inline = false });
            }

            string title = success ? 
                (action == "hidden" ? "Item hidden" : "Item retrieved") : 
                $"{action} failed";

            var embed = new
            {
                title = title,
                color = success ? 0x00ff00 : 0xff0000,
                fields = fields.ToArray()
            };

            await SendWebhook(embed);
        }

        private static async Task SendWebhook(object embed)
        {
            try
            {
                var payload = new
                {
                    username = Plugin.Instance?.Config.DiscordWebhookUsername ?? "SCP Facility Security",
                    avatar_url = string.IsNullOrEmpty(Plugin.Instance?.Config.DiscordWebhookAvatarUrl) ? null : Plugin.Instance?.Config.DiscordWebhookAvatarUrl,
                    embeds = new[] { embed }
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(Plugin.Instance?.Config.DiscordWebhookUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warn($"Discord webhook failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Discord webhook error: {ex.Message}");
            }
        }

        public static IEnumerator<float> LogShivCreationCoroutine(Player player, bool success, float damage = 0f)
        {
            yield return Timing.WaitForOneFrame;
            _ = Task.Run(async () => await LogShivCreation(player, success, damage));
        }

        public static IEnumerator<float> LogShivAttackCoroutine(Player attacker, Player target, float damage, bool consumed)
        {
            yield return Timing.WaitForOneFrame;
            _ = Task.Run(async () => await LogShivAttack(attacker, target, damage, consumed));
        }

        public static IEnumerator<float> LogSearchEventCoroutine(Player searcher, Player target, bool foundShiv, bool confiscated)
        {
            yield return Timing.WaitForOneFrame;
            _ = Task.Run(async () => await LogSearchEvent(searcher, target, foundShiv, confiscated));
        }

        public static IEnumerator<float> LogAdminCommandCoroutine(Player admin, string command, string action, bool success)
        {
            yield return Timing.WaitForOneFrame;
            _ = Task.Run(async () => await LogAdminCommand(admin, command, action, success));
        }

        public static IEnumerator<float> LogHideUnhideEventCoroutine(Player player, string action, bool success, string reason = "")
        {
            yield return Timing.WaitForOneFrame;
            _ = Task.Run(async () => await LogHideUnhideEvent(player, action, success, reason));
        }
    }
}
