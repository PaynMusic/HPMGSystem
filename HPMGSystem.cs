IMyRadioAntenna missileAntenna;
IMyCameraBlock cameraBlock;
IMySensorBlock sensorBlock;
List<IMyTerminalBlock> launchThrusters = new List<IMyTerminalBlock>();
List<IMyWarhead> warheads = new List<IMyWarhead>();
IMyRemoteControl remoteControl;
Vector3D targetPosition;
bool isLaunched = false;

public Program()
{
    missileAntenna = GridTerminalSystem.GetBlockWithName("Missile Antenna") as IMyRadioAntenna;
    cameraBlock = GridTerminalSystem.GetBlockWithName("Camera Block Name") as IMyCameraBlock;
    sensorBlock = GridTerminalSystem.GetBlockWithName("Sensor Block Name") as IMySensorBlock;
    remoteControl = GridTerminalSystem.GetBlockWithName("PM- Guidance System") as IMyRemoteControl;

    GridTerminalSystem.GetBlockGroupWithName("PM- Launch Thrusters").GetBlocks(launchThrusters);
    GridTerminalSystem.GetBlockGroupWithName("PM- Power System").GetBlocksOfType(warheads);

    // Enable raycasting for the camera
    cameraBlock.EnableRaycast = true;

    // Configure the sensor
    sensorBlock.LeftExtend = 1;
    sensorBlock.RightExtend = 1;
    sensorBlock.TopExtend = 1;
    sensorBlock.BottomExtend = 1;
    sensorBlock.FrontExtend = 1;
    sensorBlock.BackExtend = 1;
    sensorBlock.DetectLargeShips = true;
    sensorBlock.DetectSmallShips = true;

    // Run the script every 10 update ticks (about 1 second)
    Runtime.UpdateFrequency = UpdateFrequency.Update20;
}

public void Main(string argument, UpdateType updateSource)
{
    string targetName = missileAntenna.CustomData;
    if (string.IsNullOrEmpty(targetName))
    {
        UpdateAntennaStatus("waiting for hit list");
        return;
    }

    bool targetIdentified = false;
    IMyBroadcastListener listener = IGC.RegisterBroadcastListener(targetName);
    while (listener.HasPendingMessage)
    {
        MyIGCMessage message = listener.AcceptMessage();
        targetPosition = (Vector3D)message.Data;
        targetIdentified = true;

        if (Vector3D.Distance(remoteControl.GetPosition(), targetPosition) <= 5000)
        {
            UpdateAntennaStatus("target locked, launching");
            ArmWarheads();
            if (!isLaunched)
            {
                ActivateLaunchThrusters();
                isLaunched = true;
            }
            break;
        }
    }

    if (targetIdentified && !isLaunched)
    {
        UpdateAntennaStatus("target identified, watching");
    }
    else if (!targetIdentified)
    {
        UpdateAntennaStatus("scanning for targets");
    }

    // Control missile after launch
    if (isLaunched)
    {
        // Increase update frequency for more frequent guidance updates
        Runtime.UpdateFrequency = UpdateFrequency.Update1;

        // Continuously update target position from latest broadcast
        IMyBroadcastListener flightListener = IGC.RegisterBroadcastListener(missileAntenna.CustomData);
        if (flightListener.HasPendingMessage)
        {
            MyIGCMessage flightMessage = flightListener.AcceptMessage();
            targetPosition = (Vector3D)flightMessage.Data;
        }

        // Update missile flight path
        remoteControl.SetAutoPilotEnabled(false);
        remoteControl.ClearWaypoints();
        remoteControl.AddWaypoint(targetPosition, "Target");
        remoteControl.SetAutoPilotEnabled(true);

        // Check for impact
        if (sensorBlock.IsActive && sensorBlock.LastDetectedEntity != null)
        {
            DetonateWarheads();
        }

        // Update the estimated countdown until impact
        UpdateAntennaStatus("In flight - " + EstimateTimeToImpact() + "s to impact");
    }
}

void UpdateAntennaStatus(string status)
{
    missileAntenna.CustomName = "Missile Antenna [" + status + "]";
}

string EstimateTimeToImpact()
{
    double distanceToTarget = Vector3D.Distance(remoteControl.GetPosition(), targetPosition);
    double speed = remoteControl.GetShipSpeed();
    return (speed > 0) ? (distanceToTarget / speed).ToString("F2") : "N/A";
}

void ActivateLaunchThrusters()
{
    foreach (var thruster in launchThrusters)
    {
        (thruster as IMyThrust).ThrustOverridePercentage = 100;
    }
}

void ArmWarheads()
{
    foreach (var warhead in warheads)
    {
        warhead.IsArmed = true;
    }
}

void DetonateWarheads()
{
    foreach (var warhead in warheads)
    {
        warhead.Detonate();
    }
}
