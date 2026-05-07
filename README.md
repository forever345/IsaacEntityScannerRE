# IsaacEntityScannerRE

WPF application for tracking spawned pickups in *The Binding of Isaac: Repentance* using shared memory communication between a native DLL hook and a C# UI layer.

The project listens for newly spawned pickups, maps them against a local JSON database, and displays detailed information in a live feed UI.

---

# Features

- Real-time pickup tracking
- Shared memory communication
- Event-based recent feed system
- Persistent seen/history collection
- Automatic recent feed cleanup
- Detailed item descriptions from JSON database
- Recent / Seen tabs
- Ring buffer based event stream
- Dynamic WPF item rendering

---

# Project Architecture

The project consists of 3 main parts:

## 1. IsaacInjector

External injector executable responsible for injecting the native DLL into the game process.

Used during application startup before shared memory initialization.

---

## 2. IsaacEntityHook.dll

Native C++ DLL injected into the game.

### Responsibilities

- Hook game entity logic
- Detect spawned pickups
- Filter supported pickup variants
- Write entity data into shared memory ring buffer

### Currently supported pickup variants

- `100` — Collectibles
- `300` — Cards
- `350` — Trinkets

The DLL publishes events using a producer-style ring buffer.

---

## 3. IsaacEntityScannerRE

Main WPF application.

### Responsibilities

- Connect to shared memory
- Read newly published entity events
- Track recent and seen pickups
- Map pickups against JSON database
- Render detailed UI feed

---

# Shared Memory System

The application uses a memory mapped file shared between the DLL and the WPF application.

The DLL writes entity events into a fixed-size ring buffer.

Each entity entry contains:

```csharp
struct EntityState
{
    int ptr;
    int type;
    int variant;
    int id;
}
The C# application consumes only newly published entries using incremental ring buffer reading.

---

# Feed System

The tracker uses two internal collections:

## Seen

Persistent history of all discovered pickups during the current session.

Implemented as:

```csharp
HashSet<PickupKey>
```

---

## Recent

Temporary live feed of recently spawned pickups.

Implemented as:

```csharp
Dictionary<PickupKey, DateTime>
```

Each recent pickup stores its last seen timestamp.

Old entries automatically expire after a configurable amount of time.

---

# UI

The UI contains two modes:

## Recent

Shows recently spawned pickups.

Entries automatically disappear after expiration.

---

## Seen

Shows all discovered pickups during the current session.

---

# Item Database

Pickup metadata is loaded from a local JSON database.

The database provides:

- Item names
- Descriptions
- Pools
- Tags
- Quality
- Pickup types

The UI formats these entries into readable item cards with detailed descriptions.

---

# Startup Flow

Application startup process:

1. Launch `IsaacInjector`
2. Inject `IsaacEntityHook.dll`
3. Wait for shared memory availability
4. Initialize shared memory connection
5. Load JSON database
6. Initialize UI manager
7. Initialize pickup tracker
8. Start polling timer
9. Read newly published entity events
10. Update recent/seen feed
11. Render UI

---

# Planned Improvements

- Diff-based UI rendering
- UI virtualization
- Manual dismiss / restore actions
- Filtering and sorting
- Better startup/error handling
- Duplicate DLL injection protection
- Ring buffer overflow detection

---

# Dependencies

- .NET / WPF
- IsaacInjector
- IsaacEntityHook.dll
- Local JSON item database

---

# Notes

This project is intended for educational and reverse engineering purposes.
