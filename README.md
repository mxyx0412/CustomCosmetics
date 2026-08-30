[**简体中文**](README_CN.md) | English

# CustomCosmetics

An Among Us custom cosmetics loader. Downloads and loads custom **Hats / Visors / Nameplates** from remote repositories, with support for multiple repositories, package grouping, and incremental downloads.

## Installation

1. Place `CosmeticsManager.dll` into `BepInEx/plugins/`
2. Launch the game once to generate the config file `BepInEx/config/com.mxyx.cosmetics.cfg`
3. Edit the config and restart the game to apply changes

## Config `BepInEx/config/com.mxyx.cosmetics.cfg`

| Option | Default | Description |
|---|---|---|
| `EnableHats` | `true` | Enable hat loading |
| `EnableVisors` | `false` | Enable visor loading |
| `EnableNamePlates` | `false` | Enable nameplate loading |
| `Repositories` | `https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master\|hat` | Repository list |

`Repositories` format: `url|flags;url|flags`, `flags` is `hat` / `visor` / `plate` (defaults to `hat`), separate multiple repositories with `;`.

Cosmetics cache directory: `{persistentDataPath}/CustomCosmetics` (`Android/data/<package>/files/CustomCosmetics` on Android), with subdirectories `CustomHats/ CustomVisors/ CustomNamePlates/`.

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

.NET 6.0 + BepInEx IL2CPP
