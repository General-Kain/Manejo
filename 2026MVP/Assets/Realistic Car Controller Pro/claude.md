# Realistic Car Controller Pro (RCCP) - Project Documentation

## Overview

**Realistic Car Controller Pro** is a modular, component-based vehicle physics system for Unity by BoneCracker Games. It provides realistic vehicle simulation with comprehensive drivetrain physics, customization systems, AI, and multi-platform input support.

**Version**: V1.91.1
**Author**: Ekrem Bugra Ozdoganlar (BoneCracker Games)
**Copyright**: 2014 - 2025

---

## Project Structure

```
Assets/Realistic Car Controller Pro/
├── Addons/                    # Integration packages (Photon, Mirror, Enter-Exit)
│   ├── Installed/             # Active addon content
│   └── Installers/            # UnityPackages for addons and shaders
├── Audio/                     # Sound effects organized by type
│   ├── Engine/                # Engine audio clips (multiple vehicle types)
│   ├── Exhaust/               # Backfire and exhaust sounds
│   ├── Gearbox/               # Gear shifting sounds
│   ├── Impact/                # Collision sounds
│   ├── Skid/                  # Tire skid sounds by surface
│   ├── Turbo/                 # Turbo and blow-off sounds
│   ├── Vehicle/               # General vehicle sounds
│   └── Wheel/                 # Wheel-specific sounds
├── Documentation/             # PDF documentation files
├── Editor/                    # Custom editor scripts and inspectors
│   ├── Customization/         # Vehicle upgrade editors
│   └── InitLoad/              # Project initialization scripts
├── InputActions/              # Unity New Input System definitions
├── Materials/                 # Material assets
│   ├── Decals/                # Vehicle decal materials
│   ├── Neon/                  # Neon underglow materials
│   ├── Particle Materials/    # Particle effect materials
│   └── Wheel Blurs/           # Motion blur materials for wheels
├── Models/                    # 3D model assets
│   ├── Prototype Vehicle/     # Sample vehicle model
│   ├── Spoilers/              # Spoiler models
│   └── Wheels/                # Wheel model pack
├── Physics Materials/         # PhysicMaterial assets for surfaces
├── Prefabs/                   # Ready-to-use prefabs
│   ├── Cameras/               # Camera prefabs
│   ├── Customization Setups/  # Upgrade system prefabs
│   ├── Misc/                  # Utility prefabs
│   ├── Particles/             # Particle effect prefabs
│   ├── Prototype/             # Sample vehicle prefabs
│   ├── Skidmarks/             # Skidmark system prefabs
│   ├── UI/                    # UI canvas prefabs
│   └── Wheels/                # Wheel prefabs
├── Resources/                 # ScriptableObject assets (loaded at runtime)
├── Scenes/                    # Demo and prototype scenes
└── Scripts/                   # All C# scripts (168 files)
    ├── AI/                    # AI driving system
    ├── Audio/                 # Audio management
    ├── Base/                  # Core base classes
    ├── Camera/                # Camera controllers
    ├── Customization/         # Customization data and triggers
    ├── Demo/                  # Demo and example scripts
    ├── Editors/               # Editor utility scripts
    ├── Event/                 # Event system
    ├── Inputs/                # Input handling
    ├── Interface/             # Core interfaces
    ├── Manager/               # Singleton managers
    ├── Others/                # Utility scripts
    ├── Scriptable Objects/    # ScriptableObject definitions
    ├── UI/                    # UI components (40+ scripts)
    ├── Upgrades/              # Vehicle upgrade system
    │   └── Managers/          # Upgrade managers
    └── Vehicle/               # Core vehicle components (30 scripts)
```

---

## Architecture

### Core Design Pattern

RCCP uses a **modular component-based architecture** where:

1. **RCCP_CarController** is the central hub that manages all subsystems
2. Each subsystem (Engine, Gearbox, Axle, etc.) is a separate **RCCP_Component**
3. Components self-register with the parent CarController using interfaces
4. ScriptableObjects provide data-driven configuration
5. Static events enable decoupled inter-component communication

### Class Hierarchy

```
MonoBehaviour
├── RCCP_MainComponent
│   └── RCCP_CarController         # Main vehicle controller
├── RCCP_Component                  # Base for all vehicle components
│   ├── RCCP_Engine                 # Power generation
│   ├── RCCP_Clutch                 # Clutch engagement
│   ├── RCCP_Gearbox                # Transmission
│   ├── RCCP_Differential           # Torque distribution
│   ├── RCCP_Axles                  # Axle manager
│   ├── RCCP_Axle                   # Individual axle
│   ├── RCCP_WheelCollider          # Wheel physics
│   ├── RCCP_Audio                  # Audio system
│   ├── RCCP_Input                  # Input receiver
│   ├── RCCP_Lights                 # Light manager
│   ├── RCCP_Light                  # Individual light
│   ├── RCCP_Stability              # ABS/ESP/TCS
│   ├── RCCP_Damage                 # Damage system
│   ├── RCCP_Particles              # Particle effects
│   ├── RCCP_AeroDynamics           # Downforce/drag
│   ├── RCCP_Customizer             # Customization manager
│   ├── RCCP_Lod                    # Level of detail
│   ├── RCCP_OtherAddons            # Addon container
│   └── [20+ more components]
├── RCCP_GenericComponent           # Lightweight base (no registration)
├── RCCP_UIComponent                # UI element base
└── Static Classes
    ├── RCCP                        # Public API
    ├── RCCP_Events                 # Event system
    ├── RCCP_SceneManager           # Scene management singleton
    ├── RCCP_InputManager           # Input singleton
    └── RCCP_SkidmarksManager       # Skidmarks singleton
```

### Component Registration System

Components automatically register with `RCCP_CarController` via `RCCP_Component.Register()`:

```csharp
// In RCCP_Component.cs - automatic registration on property access
public RCCP_CarController CarController {
    get {
        if (_carController == null) {
            _carController = GetComponentInParent<RCCP_CarController>(true);
            if (_carController != null)
                Register(_carController, this);
        }
        return _carController;
    }
}
```

### Interfaces

| Interface | Purpose |
|-----------|---------|
| `IRCCP_Component` | Standard vehicle component interface |
| `IRCCP_UpgradeComponent` | Upgrade/customization component interface |
| `IRCCP_LoadoutComponent` | Loadout/save system interface |

---

## Drivetrain System

### Power Flow

```
RCCP_Engine
    ↓ (producedTorqueAsNM)
RCCP_Clutch
    ↓ (clutch engagement)
RCCP_Gearbox
    ↓ (gear ratio applied)
RCCP_Differential
    ↓ (torque split)
RCCP_Axle (left/right)
    ↓
RCCP_WheelCollider
```

### Key Vehicle State Variables (RCCP_CarController)

```csharp
// Engine state
float engineRPM;                    // Current RPM
float minEngineRPM, maxEngineRPM;   // RPM limits
bool engineRunning, engineStarting; // Engine state

// Transmission
int currentGear;                    // Current gear index
float currentGearRatio;             // Current gear ratio
float differentialRatio;            // Final drive ratio
bool shiftingNow, NGearNow;         // Gear state

// Speed
float speed;                        // km/h (signed, forward positive)
float absoluteSpeed;                // km/h (unsigned)
float wheelRPM2Speed;               // Speed from wheel RPM
float maximumSpeed;                 // Calculated max speed

// Torque
float producedEngineTorque;         // Engine output (Nm)
float producedGearboxTorque;        // After gearbox (Nm)
float producedDifferentialTorque;   // After diff (Nm)

// Inputs (from components)
float throttleInput_V, brakeInput_V, steerInput_V;
float handbrakeInput_V, clutchInput_V, nosInput_V;

// Inputs (from player)
float throttleInput_P, brakeInput_P, steerInput_P;
float handbrakeInput_P, clutchInput_P, nosInput_P;

// Lights
bool lowBeamLights, highBeamLights;
bool indicatorsLeftLights, indicatorsRightLights, indicatorsAllLights;
```

---

## Core Components Reference

### RCCP_Engine

Main power generator. Produces torque based on RPM and throttle.

**Key Properties:**
- `engineRPM`, `minEngineRPM`, `maxEngineRPM` - RPM values
- `maximumTorqueAsNM` - Peak torque in Newton-meters
- `NMCurve` - AnimationCurve for torque vs RPM
- `turboCharged`, `maxTurboChargePsi` - Turbo simulation
- `engineRevLimiter` - Rev limiter enabled
- `maximumSpeed` - Target top speed (auto-calculates diff ratio)

**Methods:**
- `StartEngine()`, `StopEngine()` - Engine control
- `SetTorqueMultiplier(float)` - Modify torque (for NOS, upgrades)

### RCCP_Gearbox

Transmission system with automatic/manual modes.

**Key Properties:**
- `transmissionType` - Automatic, Manual, or SemiAutomatic
- `gearRatios[]` - Array of gear ratios (index 0 = reverse)
- `currentGear` - Current gear index
- `shiftingTime` - Time to shift gears
- `shiftThreshold` - RPM threshold for auto-shifting

### RCCP_Axle

Manages a pair of wheels (left/right).

**Key Properties:**
- `isPower`, `isBrake`, `isSteer`, `isHandbrake` - Axle functions
- `maxSteerAngle` - Maximum steering angle
- `antirollForce` - Anti-roll bar strength
- `leftWheelCollider`, `rightWheelCollider` - Wheel references

### RCCP_WheelCollider

Wrapper around Unity's WheelCollider with enhanced features.

**Key Properties:**
- `wheelModel` - Visual wheel transform
- `camber`, `caster` - Wheel alignment
- `ForwardSlip`, `SidewaysSlip` - Current slip values
- `wheelTemperature` - Tire temperature simulation
- `isGrounded` - Ground contact state

### RCCP_Stability

Driving assistance systems.

**Key Properties:**
- `ABS`, `ESP`, `TCS` - Enable/disable each system
- `steeringHelper`, `tractionHelper` - Additional helpers
- `steerHelperStrength`, `tractionHelperStrength` - Helper strengths

### RCCP_Input

Receives player input for the vehicle.

**Key Properties:**
- `throttleInput`, `brakeInput`, `steerInput` - Input values 0-1
- `handbrakeInput`, `clutchInput`, `nosInput` - Additional inputs
- `steeringCurve` - Speed-sensitive steering curve
- `counterSteering` - Enable counter-steer assistance

---

## Public API (RCCP Static Class)

Located at: `Scripts/RCCP.cs`

### Vehicle Spawning
```csharp
// Spawn a vehicle prefab
RCCP_CarController vehicle = RCCP.SpawnRCC(
    vehiclePrefab,      // RCCP_CarController prefab
    position,           // World position
    rotation,           // World rotation
    registerAsPlayer,   // Set as active player vehicle
    isControllable,     // Can receive input
    isEngineRunning     // Start engine immediately
);
```

### Vehicle Registration
```csharp
// Register as player vehicle
RCCP.RegisterPlayerVehicle(vehicle);
RCCP.RegisterPlayerVehicle(vehicle, isControllable);
RCCP.RegisterPlayerVehicle(vehicle, isControllable, engineState);

// Deregister player vehicle
RCCP.DeRegisterPlayerVehicle();
```

### Vehicle Control
```csharp
// Set controllable state
RCCP.SetControl(vehicle, true/false);

// Set external control (AI/scripted)
RCCP.SetExternalControl(vehicle, true/false);

// Engine control
RCCP.SetEngine(vehicle, true/false);

// Transmission type
RCCP.SetAutomaticGear(vehicle, true/false);
RCCP.SetAutomaticGear(vehicle, RCCP_Gearbox.TransmissionType.Automatic);
```

### Recording/Replay
```csharp
RCCP.StartStopRecord(vehicle);
RCCP.StartStopReplay(vehicle);
RCCP.StartStopReplay(vehicle, recordedClip);
RCCP.StopRecordReplay(vehicle);
```

### Utility Functions
```csharp
// Teleport vehicle
RCCP.Transport(position, rotation);
RCCP.Transport(vehicle, position, rotation);
RCCP.Transport(vehicle, position, rotation, resetVelocity);

// Camera control
RCCP.ChangeCamera();

// Repair vehicle
RCCP.Repair();
RCCP.Repair(vehicle);

// Clean skidmarks
RCCP.CleanSkidmarks();
RCCP.CleanSkidmarks(index);

// Behavior preset
RCCP.SetBehavior(behaviorIndex);

// Mobile controller type
RCCP.SetMobileController(RCCP_Settings.MobileController.TouchScreen);
```

---

## Event System (RCCP_Events)

Located at: `Scripts/Event/RCCP_Events.cs`

### Available Events

```csharp
// Vehicle lifecycle
RCCP_Events.OnRCCPSpawned += (RCCP_CarController vehicle) => { };
RCCP_Events.OnRCCPDestroyed += (RCCP_CarController vehicle) => { };

// AI vehicles
RCCP_Events.OnRCCPAISpawned += (RCCP_CarController vehicle) => { };
RCCP_Events.OnRCCPAIDestroyed += (RCCP_CarController vehicle) => { };

// Collisions
RCCP_Events.OnRCCPCollision += (RCCP_CarController vehicle, Collision collision) => { };

// Camera
RCCP_Events.OnRCCPCameraSpawned += (RCCP_Camera cam) => { };

// UI
RCCP_Events.OnRCCPUISpawned += (RCCP_UIManager ui) => { };
RCCP_Events.OnRCCPUIDestroyed += (RCCP_UIManager ui) => { };
RCCP_Events.OnRCCPUIInformer += (string text) => { };

// Settings
RCCP_Events.OnBehaviorChanged += () => { };
RCCP_Events.OnVehicleChanged += () => { };
RCCP_Events.OnVehicleChangedToVehicle += (RCCP_CarController vehicle) => { };
```

### Firing Events

```csharp
RCCP_Events.Event_OnRCCPSpawned(vehicle);
RCCP_Events.Event_OnRCCPCollision(vehicle, collision);
RCCP_Events.Event_OnBehaviorChanged();
// etc.
```

---

## Configuration Assets (Resources/)

### RCCP_Settings

Main configuration ScriptableObject. Access via `RCCP_Settings.Instance`.

**Key Settings:**
- `multithreading` - Enable multithreading (platform-dependent)
- `overrideFPS`, `maxFPS` - FPS limiting
- `overrideFixedTimeStep`, `fixedTimeStep` - Physics timestep
- `maxAngularVelocity` - Rigidbody angular velocity limit
- `mobileControllerEnabled`, `mobileController` - Mobile input type
- `useTelemetry`, `useInputDebugger` - Debug features
- `autoReset` - Auto-reset upside-down vehicles

**Behavior Types:**
- Presets for stability settings (ABS, ESP, TCS)
- Steering curves and sensitivity
- Friction curve parameters
- Differential type defaults

**Layers:**
- `RCCPLayer` - "RCCP_Vehicle"
- `RCCPWheelColliderLayer` - "RCCP_WheelCollider"
- `RCCPDetachablePartLayer` - "RCCP_DetachablePart"
- `RCCPPropLayer` - "RCCP_Prop"
- `RCCPObstacleLayer` - "RCCP_Obstacle"

### RCCP_GroundMaterials

Surface friction database. Access via `RCCP_GroundMaterials.Instance`.

- Contains friction settings per PhysicMaterial
- Forward/sideways stiffness multipliers
- Associated audio clips for skid sounds

### RCCP_ChangableWheels

Wheel prefab library for customization.

### RCCP_Records

Recorded vehicle replays storage.

### RCCP_DemoVehicles / RCCP_DemoContent / RCCP_DemoScenes

Demo content references.

---

## Input System

### Input Flow

```
RCCP_InputManager (Singleton)
    ↓ (reads New Input System or legacy)
RCCP_Input (per-vehicle component)
    ↓ (optional override for AI/external)
RCCP_CarController (applies to drivetrain)
```

### Mobile Controllers

Configured in `RCCP_Settings.mobileController`:
- `TouchScreen` - On-screen buttons
- `Gyro` - Accelerometer steering
- `SteeringWheel` - Virtual steering wheel
- `Joystick` - Virtual joystick

### Overriding Inputs

```csharp
// In your script
vehicle.Inputs.overridePlayerInputs = true;
vehicle.Inputs.throttleInput = 0.8f;
vehicle.Inputs.steerInput = -0.5f;
// etc.
```

---

## Upgrade/Customization System

Located at: `Scripts/Upgrades/`

### Upgrade Components

| Component | Purpose |
|-----------|---------|
| `RCCP_VehicleUpgrade_Engine` | Engine power upgrades |
| `RCCP_VehicleUpgrade_Brake` | Brake performance |
| `RCCP_VehicleUpgrade_Handling` | Suspension/handling |
| `RCCP_VehicleUpgrade_Speed` | Top speed upgrades |
| `RCCP_VehicleUpgrade_Wheel` | Wheel swapping |
| `RCCP_VehicleUpgrade_Paint` | Paint colors |
| `RCCP_VehicleUpgrade_Spoiler` | Spoiler attachments |
| `RCCP_VehicleUpgrade_Siren` | Police siren |
| `RCCP_VehicleUpgrade_Decal` | Vehicle decals |
| `RCCP_VehicleUpgrade_Neon` | Underglow lights |

### Upgrade Managers

- `RCCP_VehicleUpgrade_UpgradeManager` - Unified upgrade interface
- `RCCP_VehicleUpgrade_PaintManager` - Paint system
- `RCCP_VehicleUpgrade_NeonManager` - Neon system
- `RCCP_VehicleUpgrade_CustomizationManager` - Overall customization

### Customization Data

```csharp
// Load/save customization
RCCP_CustomizationLoadout loadout = new RCCP_CustomizationLoadout();
loadout.engineLevel = 3;
loadout.brakeLevel = 2;
loadout.paintColor = Color.red;
// Apply to vehicle...
```

---

## AI System

Located at: `Scripts/AI/`

### Components

| Script | Purpose |
|--------|---------|
| `RCCP_AI` | Main AI controller |
| `RCCP_AIWaypointsContainer` | Waypoint path holder |
| `RCCP_Waypoint` | Individual waypoint |
| `RCCP_AIBrakeZonesContainer` | Brake zone holder |
| `RCCP_AIBrakeZone` | Speed limit zone |
| `RCCP_AIDynamicObstacleAvoidance` | Runtime obstacle detection |

### AI Setup

1. Create `RCCP_AIWaypointsContainer` with child `RCCP_Waypoint` objects
2. Optionally add `RCCP_AIBrakeZonesContainer` for corners
3. Add `RCCP_AI` component to vehicle's OtherAddons
4. Assign waypoint container reference

---

## Camera System

Located at: `Scripts/Camera/`

### Camera Types

| Prefab | Class | Purpose |
|--------|-------|---------|
| RCCP_Camera | `RCCP_Camera` | Main follow/orbit camera |
| RCCP_HoodCamera | `RCCP_HoodCamera` | First-person hood view |
| RCCP_WheelCamera | `RCCP_WheelCamera` | Wheel chase camera |
| RCCP_CinematicCamera | `RCCP_CinematicCamera` | Animated cinematic shots |
| RCCP_FixedCamera | `RCCP_FixedCamera` | Static scene cameras |

### Camera Control

```csharp
// Switch camera modes
RCCP.ChangeCamera();

// Access current camera
RCCP_Camera mainCamera = RCCP_SceneManager.Instance.activePlayerCamera;
```

---

## Editor Tooling

Located at: `Editor/`

### Key Editor Windows

| Tool | Menu Path | Purpose |
|------|-----------|---------|
| Setup Wizard | Tools > BCG > RCCP > Setup Wizard | 6-step vehicle setup |
| Welcome Window | Tools > BCG > RCCP > Welcome | Onboarding and addons |
| Collider Generator | Tools > BCG > RCCP > Collider Generator | Create body colliders |
| Light Setup Wizard | Tools > BCG > RCCP > Light Setup | Configure vehicle lights |
| Render Pipeline Converter | Tools > BCG > RCCP > Pipeline Converter | URP/HDRP conversion |

### Vehicle Creation

**Quick Setup (via code):**
```csharp
// Editor script
RCCP_CreateNewVehicle.Create(vehicleGameObject, "MyVehicle");
```

**Wizard Setup:**
1. Tools > BCG > RCCP > Setup Wizard
2. Select vehicle body mesh
3. Configure mass, handling type
4. Assign wheels (auto-detected)
5. Set suspension (auto-calculated)
6. Configure engine torque
7. Add optional components

### Custom Inspectors

All major components have custom editors:
- `RCCP_CarControllerEditor` - Main vehicle inspector
- `RCCP_EngineEditor`, `RCCP_GearboxEditor`, etc.
- Validation warnings and quick-fix buttons
- Component linking and UnityEvent wiring

---

## Scripting Symbols

Conditional compilation via `RCCP_AddonDefineManager`:

| Symbol | Purpose |
|--------|---------|
| `BCG_RCCP` | Main RCCP installed |
| `RCCP_PHOTON` | Photon PUN 2 integration |
| `RCCP_MIRROR` | Mirror networking integration |
| `BCG_ENTEREXIT` | Enter/Exit system integration |
| `BCG_URP` | Universal Render Pipeline |
| `BCG_HDRP` | High Definition Render Pipeline |

---

## Common Patterns

### Accessing Vehicle Components

```csharp
RCCP_CarController vehicle = GetComponent<RCCP_CarController>();

// Direct access (lazy-loaded)
vehicle.Engine.engineRPM;
vehicle.Gearbox.currentGear;
vehicle.Stability.ABS;

// Through OtherAddonsManager
vehicle.OtherAddonsManager.Nos.nosInUse;
vehicle.OtherAddonsManager.AI.currentWaypoint;
vehicle.OtherAddonsManager.Recorder.Record();

// Axle access
vehicle.FrontAxle.steerAngle;
vehicle.RearAxle.leftWheelCollider.ForwardSlip;
vehicle.PoweredAxles; // List<RCCP_Axle>
vehicle.BrakedAxles;
vehicle.SteeredAxles;
vehicle.AllWheelColliders; // RCCP_WheelCollider[]
```

### Creating a Custom Component

```csharp
public class MyCustomComponent : RCCP_Component {

    public override void Awake() {
        base.Awake(); // Registers with CarController
    }

    private void FixedUpdate() {
        if (!CarController) return;

        // Access vehicle data
        float speed = CarController.speed;
        float rpm = CarController.engineRPM;

        // Modify vehicle
        CarController.Engine.SetTorqueMultiplier(1.5f);
    }

    public void Reload() {
        // Reset state when enabled/disabled
    }
}
```

### Listening to Events

```csharp
void OnEnable() {
    RCCP_Events.OnRCCPSpawned += OnVehicleSpawned;
    RCCP_Events.OnRCCPCollision += OnVehicleCollision;
}

void OnDisable() {
    RCCP_Events.OnRCCPSpawned -= OnVehicleSpawned;
    RCCP_Events.OnRCCPCollision -= OnVehicleCollision;
}

void OnVehicleSpawned(RCCP_CarController vehicle) {
    Debug.Log($"Vehicle spawned: {vehicle.name}");
}

void OnVehicleCollision(RCCP_CarController vehicle, Collision collision) {
    Debug.Log($"Collision force: {collision.impulse.magnitude}");
}
```

---

## Performance Considerations

### Optimization Settings

In `RCCP_Settings`:
- `multithreading` - Use for large vehicle counts
- `fixedTimeStep` - Balance physics accuracy vs performance
- `maxAngularVelocity` - Stability vs responsiveness

### LOD System

`RCCP_Lod` component manages:
- Wheel rotation updates
- Particle effect quality
- Audio source count
- Physics update frequency

### Skidmark Optimization

`RCCP_SkidmarksManager` pools skidmark meshes:
- Configure max marks per surface type
- Call `RCCP.CleanSkidmarks()` periodically

---

## Troubleshooting

### Common Issues

1. **Vehicle won't move**
   - Check `canControl` is true
   - Verify engine is running (`engineRunning`)
   - Check `RCCP_Input` component exists
   - Verify at least one axle has `isPower = true`

2. **Wheels not touching ground**
   - Check WheelCollider radius and suspension distance
   - Verify ground has collider and correct layer

3. **Car flips or unstable**
   - Lower center of mass on Rigidbody
   - Increase `antirollForce` on axles
   - Enable stability helpers

4. **Input not working**
   - Check `RCCP_SceneManager` exists in scene
   - Verify Input System package if using new input
   - Check mobile controller settings for mobile builds

### Debug Tools

- Enable `useTelemetry` in RCCP_Settings
- Enable `useInputDebugger` for input visualization
- Use `RCCP_Telemetry` prefab for runtime stats

---

## File Locations Quick Reference

| What | Path |
|------|------|
| Main API | `Scripts/RCCP.cs` |
| Car Controller | `Scripts/Vehicle/RCCP_CarController.cs` |
| Base Component | `Scripts/Base/RCCP_Component.cs` |
| Settings | `Scripts/Scriptable Objects/RCCP_Settings.cs` |
| Events | `Scripts/Event/RCCP_Events.cs` |
| Input Manager | `Scripts/Inputs/RCCP_InputManager.cs` |
| Scene Manager | `Scripts/Manager/RCCP_SceneManager.cs` |
| Engine | `Scripts/Vehicle/RCCP_Engine.cs` |
| Gearbox | `Scripts/Vehicle/RCCP_Gearbox.cs` |
| Setup Wizard | `Editor/RCCP_SetupWizard.cs` |
| Vehicle Creator | `Editor/RCCP_CreateNewVehicle.cs` |
| Settings Asset | `Resources/RCCP_Settings.asset` |
| Ground Materials | `Resources/RCCP_GroundMaterials.asset` |
