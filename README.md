# Space Engineers HPMGSystem
Homing Party Missile Guidance system script for use in the sandbox game, space engineers.

## Overview

This script controls a missile in Space Engineers, enabling it to seek and destroy targets specified by the player. The script uses antenna broadcasts to identify targets, confirms them with a camera block's raycast, and then guides the missile to the target using a remote control block.

## Prerequisites

- **Missile Setup**: The missile should have a Remote Control block, a Camera block, a Sensor block, and an Antenna. It should also have thrusters and warheads.
- **Naming**: Blocks must be named correctly to match the names used in the script.
- **Block Grouping**: Thrusters and warheads should be grouped as per the names specified in the script.

## Script Components

1. **Initialization**: Setting up block references and configuring sensor and camera.
2. **Target Acquisition**: Listening for broadcasts matching antenna custom data.
3. **Launch Sequence**: Launching the missile when the target is within 5km.
4. **Guidance System**: Updating the missile's trajectory towards the target.
5. **Impact Detection**: Using the sensor block to detect proximity to the target and detonate the warheads.

## Usage

1. **Setting a Target**: Enter the target's broadcast name into the custom data field of the missile's antenna.
2. **Launching the Missile**: The script automatically launches the missile when a matching target is within 5km.
3. **Monitoring Status**: The missile's antenna updates its broadcast name to reflect the current status.

## Integration as a Library/API

### Overview

This script can be used as a library or API to control missiles from other scripts or systems within the game.

### Example 1: Triggering the Missile from Another Block

Suppose you have a control center that monitors enemy broadcasts. You can trigger the missile script by writing the enemy's name into the missile antenna's custom data.

```csharp
IMyRadioAntenna missileAntenna = GridTerminalSystem.GetBlockWithName("Missile Antenna") as IMyRadioAntenna;
missileAntenna.CustomData = "Enemy Broadcast Name";
```

### Example 2: Integrating with a Defense System

In a defense system script, you can incorporate the missile script to automatically respond to certain threats.

```csharp
// Assuming the missile script is encapsulated in a function `LaunchMissile`
void RespondToThreat(string threatBroadcastName)
{
    LaunchMissile(threatBroadcastName);
}
```

## Troubleshooting and Tips

- **Ensure Block Names Match**: Check that all block names in the script match the names in the game.
- **Power Requirements**: Make sure your missile has enough power for the camera block's raycast.
- **Testing**: Always test the script in a safe environment before actual use.

---
