# ProtoFlux Node Display Mod

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://store.steampowered.com/app/2519830/Resonite/)

![image](https://github.com/user-attachments/assets/39e99d72-ac40-4818-b7df-ea3054e86de7)

## What is this?

This mod changes ProtoFlux operator display from English names to NeosVR-style symbolic notation in the ProtoFlux browser.

For example:
- `Add` → `Add (+)`
- `LessThan` → `LessThan (<)`
- `RotateLeft` → `RotateLeft (ROL)`
- `ValueSquare` → `ValueSquare (x²)`

## How to install

1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader)
2. Download the latest release, and extract the zip file's contents to your mods folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader.
3. Start the game!

The mod will automatically enhance ProtoFlux node names in the ComponentSelector (node browser) with NeosVR-style symbols.

## Features

- Converts 25+ ProtoFlux operators to NeosVR-style symbols
- Supports both generic and non-generic node types (e.g., `ValueDec<T>`)
- Only affects text within ComponentSelector context (doesn't modify other UI text)
- Includes mathematical symbols: `+`, `-`, `×`, `÷`, `%`, `x²`, `x³`, `1/x`, etc.
- Includes comparison operators: `<`, `>`, `≤`, `≥`, `==`, `!=`, `≈`
- Includes logical operators: `&`, `|`, `^`, `!`
- Includes bitwise operations: `<<`, `>>`, `ROL`, `ROR`

## ⚠️ Important Notes

**This mod is still in development and may be unstable. Use at your own risk.**

### Known Issues
- **Performance Impact**: Frame rate drops may occur in areas with many users due to text processing overhead
- **Compatibility**: May conflict with other UI-modifying mods

### Recent Fixes
- **v1.1.0**: Make sure arithmetic and comparison operators are used in sequence.
Prevent unintended replacements during type lookups.

- **v1.0.1**: Fixed significant frame rate drops in multiplayer environments
- **v1.0.0**: Optimized text processing to reduce performance impact

