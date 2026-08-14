[**简体中文**](README_CN.md) | English

# CustomCosmetics

An Among Us custom cosmetics loader. Downloads and loads custom **Hats / Visors / Nameplates** from remote repositories, with support for multiple repositories, package grouping, and incremental downloads.

## Installation

1. Place `CosmeticsManager.dll` into `BepInEx/plugins/`
2. Launch the game once to generate the config file `Cosmetics/config.yml` (in the game directory)
3. Edit the config and restart the game to apply changes

## Config File `Cosmetics/config.yml`

```yaml
cosmetics:
  # Local-only mode: no network downloads, reads the local cache only
  local: false
hats:
  enabled: true       # Enable hat loading
visors:
  enabled: false      # Enable visor loading
nameplates:
  enabled: false      # Enable nameplate loading

repositories:
  # Repository URL
  - url: "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master"
    # Local cache directory name (optional)
    alias: "TheOtherHats"
    # Which cosmetic types to load from this repository
    hats: true
    visors: false
    nameplates: false
    # Custom config file names (optional, defaults: CustomHats.json / CustomVisors.json / CustomNamePlates.json)
    hatsFile: "MyHats.json"
    visorsFile: "MyVisors.json"
    platesFile: "MyPlates.json"
    # Custom resource subdirectories (optional, defaults: hats/ visors/ nameplates/)
    hatsDir: "hats"
    visorsDir: "visors"
    platesDir: "nameplates"
```

Multiple repositories can be configured; same types are merged and displayed grouped by package.

## Repository Config Format

Each repository root holds three config files, with resource files in the matching subdirectories (`{url}/{resDir}/{filename}`).

### `CustomHats.json`

```json
{
  "packages": [                                        // package definitions (optional)
    { "package": "HatsPack",                           // package ID
      "displayName": "My Hats",                        // package name shown in the UI
      "priority": 50 }                                 // sort weight (higher = later)
  ],
  "hats": [
    {
      "name": "Name",                                  // cosmetic name
      "author": "Author",                              // author
      "package": "HatsPack",                           // owning package ID
      "resource": "example.png",                       // main image
      "climbresource": "example_climb.png",            // climbing animation (optional)
      "backresource": "example_back.png",              // back layer image (optional)
      "flipresource": "example_flip.png",              // flipped image (optional)
      "backflipresource": "example_back_flip.png",     // flipped back image (optional)
      "adaptive": false,                               // match player color
      "bounce": false,                                 // bouncing animation
      "behind": false,                                 // render behind the player
      "autoscale": true,                               // auto-scale to the 300px base (default on)
      "reshasha": "",                                  // main image MD5
      "reshashb": "",                                  // back MD5
      "reshashc": "",                                  // climb MD5
      "reshashf": "",                                  // flip MD5
      "reshashbf": ""                                  // back_flip MD5
    }
  ]
}
```

### `CustomVisors.json`

```json
{
  "packages": [                                        // package definitions (optional)
    { "package": "VisorPack", "displayName": "My Visors", "priority": 50 }
  ],
  "visors": [
    {
      "name": "Name",                                  // cosmetic name
      "author": "Author",                              // author
      "package": "VisorPack",                          // owning package ID
      "resource": "visor.png",                         // main image
      "flipresource": "visor_flip.png",                // flipped image (optional)
      "behindHats": false,                             // render behind the hat
      "adaptive": false,                               // match player color
      "autoscale": true,                               // auto-scale to the 300px base (default on)
      "reshasha": "",                                  // main image MD5
      "reshashf": ""                                   // flip MD5
    }
  ]
}
```

### `CustomNamePlates.json`

```json
{
  "packages": [                                        // package definitions (optional)
    { "package": "PlatePack", "displayName": "My NamePlates", "priority": 50 }
  ],
  "nameplates": [
    {
      "name": "Name",                                  // cosmetic name
      "author": "Author",                              // author
      "package": "PlatePack",                          // owning package ID
      "resource": "plate.png",                         // main image
      "reshasha": ""                                   // main image MD5
    }
  ]
}
```

## Image Specifications

| Type | Base Size | Scaling |
|---|---|---|
| Hat | 300×375 | `autoscale` on by default, larger images auto-scale to the base display size |
| Visor | 300×375 | `autoscale` on by default, larger images auto-scale to the base display size |
| Nameplate | 275×68 | fixed auto-scale |

## FAQ

**Full re-download on every launch?**

When the MD5 fields (`reshasha` etc.) in the config don't match the actual file MD5 (or are missing), the file is considered "needs download". Fill in the real MD5 values so only changed files are downloaded.

## Build

.NET 6.0 + BepInEx IL2CPP, depends on YamlDotNet.
