# Shiv Plugin

Adds shiv combat to SCP SL

## What it does

Players can creative shivs by looking at a wall, running appropriate commands and then a slim chance of gaining a shiv. This can be used to balance out roleplay servers or other servers in general with how Class D are treated. 

## Commands

**Player Commands:**
- `.shiv` - Try to craft a shiv (look at a wall)
- `.hide` - Hide your shiv
- `.unhide` - Get your hidden shiv back
- `.search` - Search someone (MTF/Guards only)

**Admin Commands:**
- `shiv enable/disable` - Turn the system on/off
- `shiv status` - Check system stats
- `shiv reset` - Clear all data
- `shiv debug enable/disable` - Toggle debug mode

## How it works

**Crafting:**
- Look at a wall within 1.5 unity units
- 1 in 8 chance to get a shiv 
- Failing deals 10 damage

**Combat:**
- Use the shiv item to attack nearby players
- Deals 40 damage + 10 HP drain per second for 3 seconds (default settings)
- Only works on human roles (no SCPs)

**Hiding:**
- Store one shiv at a time
- Can't hide while cuffed
- Use `.unhide` to get it back

**Searching:**
- Guards and MTF can search players
- Takes 3 seconds, both players must stay close
- 50/50 chance to find hidden shivs
- Found shivs go to the searcher

## Configuration

Most settings are self-explanatory. Key ones:

- `WallDetectionDistance` - How close you need to be to walls
- `SuccessChance` - 1 in X chance to craft successfully
- `ShivDamage` - Damage dealt by attacks
- `HpDrainRate` - HP lost per second from bleeding
- `SearchDetectionChance` - Chance to find hidden shivs (0.0 to 1.0)
- `EnableDiscordLogging` - Turn on Discord webhooks


## Notes


- All actions have cooldowns to prevent spam
- Discord logging is optional but useful for monitoring

Built for Exiled 9.9.2. Should work on most SCP:SL servers.
