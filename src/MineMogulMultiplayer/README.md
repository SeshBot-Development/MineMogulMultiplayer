# MineMogul Multiplayer

A **BepInEx 6** mod that adds peer-to-peer cooperative multiplayer to [Mine Mogul](https://store.steampowered.com/app/2680800/Mine_Mogul/) via **Steam Networking**.

## Features

- **Steam P2P Lobby** — Host or join sessions through an in-game multiplayer panel (press **F9** or use the menu buttons). No port-forwarding required.
- **Full World Sync** — Economy, research, quests, buildings, conveyors, machines, mining, and ore positions are kept in sync between host and clients.
- **Mid-Game Joins** — New players can join an active session; the host automatically saves and reloads to bring them up to speed.
- **Remote Players** — See other players' characters moving and interacting in the world.
- **Force Resync** — Host can trigger a full save→reload→snapshot cycle to fix any desync.
- **Kick Support** — Host can remove players from the session.
- **Event Log** — On-screen feed of multiplayer events (joins, leaves, syncs, etc.).
- **Auto-Updater** — The mod checks for new releases on GitHub and updates itself automatically on game launch.
- **Configurable** — Player name and tick rate can be set in the BepInEx config.

## Requirements

| Dependency | Version |
|---|---|
| [Mine Mogul](https://store.steampowered.com/app/2680800/Mine_Mogul/) | Latest |
| [BepInEx 6](https://github.com/BepInEx/BepInEx) (Unity Mono, x64) | 6.0.0-be.697+ |

## Installation

### Easy — Download the bundle

1. Grab the latest `MineMogulMultiplayer-vX.X.X-BepInEx-bundle.zip` from [Releases](https://github.com/SeshBot-Development/MineMogulMultiplayer/releases).
2. Extract the ZIP into your Mine Mogul game folder (e.g. `C:\Program Files (x86)\Steam\steamapps\common\Mine Mogul\`).
3. Launch the game.

### Manual — DLL only

1. Install BepInEx 6 (Unity Mono, x64) into Go to your Mine Mogul game folder.
2. Download `MineMogulMultiplayer.dll` from [Releases](https://github.com/SeshBot-Development/MineMogulMultiplayer/releases).
3. Place it in `BepInEx\plugins\MineMogulMultiplayer\`.
4. Launch the game.

## Usage

1. Open the multiplayer panel with **F9** or through the pause/main menu.
2. **Host**: Click **Host Session** to create a lobby. Share the lobby info with friends.
3. **Client**: Click **Join Session** and enter the host's Steam ID, or join through Steam friends.
4. Both players must be on the same save/world state for initial sync to work correctly.

## Building from Source

```
git clone https://github.com/SeshBot-Development/MineMogulMultiplayer.git
cd MineMogulMultiplayer
```

1. Edit `MineMogulMultiplayer.csproj` and set `<GameDir>` to your Mine Mogul install path.
2. Build:
   ```
   dotnet build --configuration Release
   ```
3. The output DLL is at `bin/Release/net472/MineMogulMultiplayer.dll`.

## Project Structure

```
Core/           Session management, state tracking, remote player manager
Models/         Network message types, game state snapshots, primitives
Networking/     Steam P2P transport layer
Patches/        Harmony patches for game systems (economy, mining, building, etc.)
Serialization/  Binary serialization and world hashing
UI/             Multiplayer panel, event log, UI factory
Updater/        GitHub-based auto-updater
Plugin.cs       BepInEx plugin entry point
```

## License

This project is provided as-is for personal and community use. Not affiliated with or endorsed by the developers of Mine Mogul.
