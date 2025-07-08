# ProtoFlux Node Display Mod

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://store.steampowered.com/app/2519830/Resonite/?l=japanese)  
Enhances ProtoFlux node display by showing both custom node names and class names.

## 機能 (Features)

FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators名前空間のノードで、NodeNameオーバーライドを持つクラスの表示を次のように変更します：

- 従来: `ROL`
- 変更後: `ROL(RotateLeft_Bool2)`

This mod targets nodes in the FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators namespace that have NodeName overrides, displaying them as:

- Before: `ROL`
- After: `ROL(RotateLeft_Bool2)`

## Usage
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader)
2. Download the latest release, and extract the zip file's contents to your mods folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader.
3. Start the game!

If you want to verify that the mod is working, check your ProtoFlux browser - nodes with NodeName overrides should now display both the custom name and the class name.

## Technical Details

- Uses HarmonyLib to patch ProtoFlux node display methods
- Detects NodeName attributes using reflection
- Caches node names for improved performance
- Targets only nodes in the FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators namespace

### Credits

Modified from VarjoEyeIntegration by ginjake. Original credits to everyone who helped with the original eye tracking mod.
