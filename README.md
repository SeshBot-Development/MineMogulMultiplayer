# MineMogul Multiplayer Mod

A host-authoritative multiplayer mod for MineMogul, built on BepInEx + Harmony + LiteNetLib.

## Architecture

```
┌──────────────────────────────────┐
│          Plugin.cs (entry)       │
├──────────────────────────────────┤
│  MultiplayerUI     (IMGUI overlay, F9 toggle)
│  SessionManager    (orchestrates host/client lifecycle)
│  ├─ NetServer      (LiteNetLib, host-side)
│  ├─ NetClient      (LiteNetLib, client-side)
│  ├─ NetSerializer  (MessagePack envelope packing)
│  └─ WorldHasher    (FNV-1a desync detection)
│  Harmony Patches   (intercept game logic per role)
│  Data Models       (GameState, Snapshots, Messages)
└──────────────────────────────────┘
```

## Prerequisites

1. **MineMogul** installed via Steam
2. **BepInEx 5.4.x** installed in the MineMogul folder
   - Download from [Thunderstore](https://thunderstore.io/c/minemogul/) or [BepInEx releases](https://github.com/BepInEx/BepInEx/releases)
   - Extract into your MineMogul game folder so `BepInEx/` sits next to `MineMogul.exe`
3. **.NET SDK 6+** (for building)
4. **dnSpy** or **ILSpy** (for reverse-engineering game assemblies)

## Setup

### 1. Update paths in the .csproj

Open `src/MineMogulMultiplayer/MineMogulMultiplayer.csproj` and update:

```xml
<GameDir>C:\Program Files (x86)\Steam\steamapps\common\MineMogul</GameDir>
```

to match your actual Steam library path.

### 2. Restore and build

```powershell
cd src\MineMogulMultiplayer
dotnet restore
dotnet build
```

The post-build step automatically copies the DLL + dependencies into:
```
<GameDir>\BepInEx\plugins\MineMogulMultiplayer\
```

### 3. Run the game

Launch MineMogul. You should see `[MineMogul Multiplayer v0.1.0 loaded]` in the BepInEx console.

Press **F9** to open the multiplayer menu.

## How it works

### Authority model: Host-authoritative

- One player **hosts** — their game runs the real simulation.
- Other players **join** — their game receives state from the host.
- Clients send inputs/RPCs. The host validates and broadcasts results.

### Network protocol

| Direction | Message | Reliability |
|-----------|---------|-------------|
| Client → Host | `PlayerInput` | Sequenced |
| Client → Host | `PlaceMachine`, `RemoveMachine`, `InteractMachine` | Reliable |
| Host → Client | `WorldDelta` | Sequenced |
| Host → Client | `WorldSnapshot` (on join) | Reliable |
| Host → Client | `HashCheck` (periodic) | Reliable |

### Harmony patching strategy

Each game system gets a pair of patches:

- **Prefix**: On clients, `return false` to skip simulation. On host, `return true` to run normally.
- **Postfix** (host only): Mark the affected entity dirty so it gets included in the next delta broadcast.

## Project structure

```
src/MineMogulMultiplayer/
├── Plugin.cs                    # BepInEx entry point
├── MultiplayerUI.cs             # IMGUI host/join/disconnect menu
├── Core/
│   ├── PluginInfo.cs            # GUID, name, version constants
│   ├── MultiplayerState.cs      # Global role tracker (Host/Client/Offline)
│   └── SessionManager.cs        # Orchestrates networking + state
├── Models/
│   ├── NetPrimitives.cs         # NetVector3, NetQuaternion
│   ├── GameState.cs             # PlayerState, MachineState, BeltState, etc.
│   ├── Snapshots.cs             # WorldSnapshot, WorldDelta, WorldHash
│   └── Messages.cs              # NetMessage envelope, RPCs, handshake
├── Networking/
│   ├── NetServer.cs             # Host-side LiteNetLib server
│   └── NetClient.cs             # Client-side LiteNetLib connection
├── Serialization/
│   ├── NetSerializer.cs         # MessagePack pack/unpack
│   └── WorldHasher.cs           # FNV-1a world state hashing
└── Patches/
    ├── MachineUpdatePatch.cs    # Template: machine tick interception
    ├── BeltTickPatch.cs         # Template: conveyor tick interception
    ├── ResourcePatch.cs         # Template: money/resource interception
    └── PlacementPatch.cs        # Template: build/destroy interception
```

## Next steps (your TODO list)

### Phase 1: Reverse-engineer the game
1. Open `Assembly-CSharp.dll` in dnSpy
2. Find the classes for: machines, belts, player controller, resource/money system, building/placement system
3. Map out which methods tick, which mutate state, which handle input

### Phase 2: Wire up Harmony patches
1. Uncomment the patch templates in `Patches/`
2. Replace placeholder class/method names with real ones from dnSpy
3. Test that singleplayer still works with patches active (all patches return `true` when offline)

### Phase 3: Wire up state extraction
1. In `SessionManager`, replace `GetLocalMoney()` and other TODOs with real game API reads
2. Build snapshot/delta builders that read actual game objects
3. Build snapshot appliers that spawn/update/destroy Unity GameObjects on clients

### Phase 4: Test multiplayer
1. Host on one machine, join from another (or two instances on localhost)
2. Verify money sync, machine placement sync, machine state sync
3. Add belt/item sync last (highest complexity)

### Phase 5: Polish
1. Add chat
2. Add player name tags
3. Handle save/load in multiplayer context
4. Desync recovery (auto-resync on hash mismatch)

## Config

Config file is auto-generated at `BepInEx/config/com.minemogul.multiplayer.cfg`:

```ini
[Network]
Port = 7777
PlayerName = Player
TickRate = 20
```

## Troubleshooting

- **DLL not loading**: Make sure BepInEx is installed correctly. Check `BepInEx/LogOutput.log`.
- **Build errors about missing references**: Update `<GameDir>` in the .csproj. The game's DLL names may differ slightly — check `MineMogul_Data/Managed/`.
- **Patches crash the game**: Start with all patches commented out. Enable one at a time.
- **Desync**: Check hash mismatch logs. The most common cause is a game system you forgot to suppress on clients.
