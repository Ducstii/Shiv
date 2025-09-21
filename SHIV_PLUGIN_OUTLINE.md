# Shiv Plugin for SCP:SL Exiled 9.9.2 - Development Outline

## Plugin Overview
A custom plugin that allows players to use a "shiv" command to potentially receive adrenaline items by interacting with walls at close range.

## Core Features

### 1. Client Command System
- **Command**: `shiv`
- **Functionality**: Players can type `.shiv` in chat to attempt to create a shiv
- **Validation**: Command only works when player is looking at a wall within close range

### 2. Raycast Wall Detection
- **Distance Check**: Very close range detection (configurable, default ~1.5 units)
- **Wall Validation**: Must be looking directly at a wall/solid surface
- **Raycast Implementation**: Uses Unity's Physics.Raycast for accurate detection

### 3. Adrenaline Item System
- **Success Rate**: 1/8 chance (12.5%) to receive adrenaline item
- **Item Type**: Adrenaline medical item
- **Serialization**: Track created items to prevent duplication

### 4. Item Serialization
- **Purpose**: Keep track of shiv-created items vs. normal items
- **Implementation**: Custom item data structure with serialization
- **Persistence**: Items maintain their "shiv-created" status

## Technical Implementation

### Plugin Structure
```
ShivPlugin/
├── Plugin.cs              # Main plugin class
├── Config.cs              # Configuration settings
├── Commands/
│   └── ShivCommand.cs     # Client command handler
├── Items/
│   └── ShivItem.cs        # Custom item serialization
└── Utils/
    └── RaycastHelper.cs   # Wall detection utilities
```

### Key Classes

#### 1. Plugin.cs
- Main plugin entry point
- Event registration and cleanup
- Configuration management

#### 2. ShivCommand.cs
- Handles `.shiv` client command
- Performs raycast wall detection
- Manages adrenaline item creation with chance system

#### 3. ShivItem.cs
- Custom item data structure
- Serialization for tracking shiv-created items
- Integration with Exiled item system

#### 4. Config.cs
- Wall detection distance
- Success chance (1/8 by default)
- Debug settings
- Enable/disable options

### Configuration Options
```csharp
public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;
    public float WallDetectionDistance { get; set; } = 1.5f;
    public int SuccessChance { get; set; } = 8; // 1 in 8 chance
    public bool AllowMultipleShivs { get; set; } = true;
    public float CooldownTime { get; set; } = 5.0f; // seconds
}
```

### Event Handling
- `PlayerCommand` event for command processing
- `PlayerItemAdded` event for item tracking
- `PlayerItemRemoved` event for cleanup

### Raycast Implementation
```csharp
private bool IsLookingAtWall(Player player, float maxDistance)
{
    Vector3 forward = player.CameraTransform.forward;
    Vector3 origin = player.CameraTransform.position;
    
    return Physics.Raycast(origin, forward, out RaycastHit hit, maxDistance) 
           && hit.collider.CompareTag("Wall");
}
```

### Item Serialization
```csharp
[Serializable]
public class ShivItemData
{
    public bool IsShivCreated { get; set; }
    public DateTime CreatedTime { get; set; }
    public string CreatorId { get; set; }
}
```

## Development Phases

### Phase 1: Basic Structure
- [x] Project setup with Exiled 9.9.2
- [x] Basic plugin class structure
- [ ] Configuration system
- [ ] Command registration

### Phase 2: Core Functionality
- [ ] Raycast wall detection
- [ ] Client command implementation
- [ ] Adrenaline item creation
- [ ] Chance system (1/8)

### Phase 3: Item Management
- [ ] Item serialization system
- [ ] Tracking shiv-created items
- [ ] Item persistence across sessions

### Phase 4: Polish & Testing
- [ ] Error handling and validation
- [ ] Debug logging system
- [ ] Performance optimization
- [ ] Extensive testing

## Usage Instructions

### For Players
1. Type `.shiv` in chat while looking at a nearby wall
2. If successful (1/8 chance), receive an adrenaline item
3. Item will be marked as shiv-created for tracking

### For Server Admins
1. Install plugin DLL in Exiled plugins folder
2. Configure settings in config file
3. Adjust wall distance, success chance, and cooldown as needed

## Technical Requirements
- **Exiled Version**: 9.9.2
- **Target Framework**: .NET Framework 4.8
- **Unity References**: UnityEngine.CoreModule, UnityEngine.PhysicsModule
- **Dependencies**: Assembly-CSharp-firstpass, Mirror.dll

## Future Enhancements
- Different wall types with different success rates
- Visual effects when shiv command succeeds
- Sound effects for immersion
- Admin commands for managing shiv-created items
- Statistics tracking for shiv usage
