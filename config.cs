using Exiled.API.Interfaces;
using System.ComponentModel;

namespace Shiv.Config;

public class Config : IConfig
{
    [Description("Whether to enable the plugin")]
    public bool IsEnabled { get; set; } = true;
    
    [Description("Whether to enable debug mode")]
    public bool Debug { get; set; } = false;
    
    [Description("Maximum distance to detect walls (Unity units)")]
    public float WallDetectionDistance { get; set; } = 1.5f;
    
    [Description("Success chance denominator (1 in X chance)")]
    public int SuccessChance { get; set; } = 8;
    
    [Description("Cooldown time between shiv attempts in seconds")]
    public float CooldownTime { get; set; } = 5.0f;
    
    [Description("Damage dealt by shiv attacks")]
    public float ShivDamage { get; set; } = 10f;
    
    [Description("HP drain rate per second from shiv attacks")]
    public float HpDrainRate { get; set; } = 10f;
    
    [Description("HP drain duration in seconds")]
    public float HpDrainDuration { get; set; } = 3f;
    
    [Description("Range for shiv attacks")]
    public float ShivAttackRange { get; set; } = 2f;
    
    [Description("Search detection chance (0.0 to 1.0)")]
    public float SearchDetectionChance { get; set; } = 0.5f;
    
    [Description("Cooldown for hide command in seconds")]
    public float HideCooldown { get; set; } = 3f;
    
    [Description("Cooldown for unhide command in seconds")]
    public float UnhideCooldown { get; set; } = 2f;
    
    [Description("Cooldown for search command in seconds")]
    public float SearchCooldown { get; set; } = 5f;
    
    [Description("Damage dealt to player when shiv crafting fails")]
    public float ShivCraftingFailureDamage { get; set; } = 10f;
    
    [Description("Whether shivs are consumed (removed) after attacking")]
    public bool ConsumeShivOnAttack { get; set; } = true;
    
    [Description("Cooldown time between shiv attacks in seconds")]
    public float AttackCooldown { get; set; } = 1.0f;
    
    [Description("Enable enhanced wall detection with multiple ray directions")]
    public bool EnhancedWallDetection { get; set; } = true;
    
    [Description("Maximum number of ray directions for wall detection")]
    public int MaxRayDirections { get; set; } = 5;
    
    [Description("Vertical surface detection threshold (0.0 = horizontal, 1.0 = vertical)")]
    public float VerticalSurfaceThreshold { get; set; } = 0.7f;
    
    [Description("Discord webhook URL for logging events")]
    public string DiscordWebhookUrl { get; set; } = "";
    
    [Description("Whether to enable Discord webhook logging")]
    public bool EnableDiscordLogging { get; set; } = false;
    
    [Description("Discord webhook username")]
    public string DiscordWebhookUsername { get; set; } = "Shiv logger";
    
    [Description("Discord webhook avatar URL")]
    public string DiscordWebhookAvatarUrl { get; set; } = "";
    
    [Description("Whether to log shiv creation events")]
    public bool LogShivCreation { get; set; } = true;
    
    [Description("Whether to log shiv attack events")]
    public bool LogShivAttacks { get; set; } = true;
    
    [Description("Whether to log search events")]
    public bool LogSearchEvents { get; set; } = true;
    
    [Description("Whether to log admin commands")]
    public bool LogAdminCommands { get; set; } = true;
}