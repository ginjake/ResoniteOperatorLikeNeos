using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace ProtoFluxNodeDisplayMod
{
	public class ProtoFluxNodeDisplayMod : ResoniteMod
	{
		public override string Name => "ProtoFluxNodeDisplayMod";
		public override string Author => "Modified from VarjoEyeIntegration";
		public override string Version => "2.0.0";
		public override string Link => "https://github.com/ginjake/ResoniteOperatorLikeNeos";
		
		public static ModConfiguration Config;
		
		[AutoRegisterConfigKey]
		public static readonly ModConfigurationKey<bool> enabled = 
			new ModConfigurationKey<bool>("enabled", "Enable enhanced node display", () => true);
		
		public override void OnEngineInit()
		{
			try
			{
				Config = GetConfiguration();
				Config.Save(true);
				
				Harmony harmony = new Harmony("net.protoflux.nodedisplay");
				
				// Apply UI component patches
				ApplyUIPatches(harmony);
				
				Msg("ProtoFlux Node Display Mod initialized with UI approach");
			}
			catch (Exception ex)
			{
				Error($"Failed to initialize ProtoFlux Node Display Mod: {ex}");
			}
		}
		
		private void ApplyUIPatches(Harmony harmony)
		{
			try
			{
				// Now we know: Text.Content is a Sync<string> field, not a property
				// We need to patch the Sync<string>.Value property setter instead
				
				// Find Sync<string> type
				var syncStringType = typeof(FrooxEngine.Sync<string>);
				Msg($"Found Sync<string> type: {syncStringType.FullName}");
				
				// Get Value property setter
				var valueProperty = syncStringType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
				if (valueProperty != null)
				{
					var valueSetter = valueProperty.GetSetMethod();
					if (valueSetter != null)
					{
						var prefix = typeof(UIPatches).GetMethod(nameof(UIPatches.SyncStringValuePrefix), BindingFlags.Static | BindingFlags.Public);
						harmony.Patch(valueSetter, prefix: new HarmonyMethod(prefix));
						Msg("Successfully patched Sync<string>.Value setter");
					}
				}
				
				// Also patch Text.OnAwake to force change existing text
				var textType = typeof(Text);
				var onAwakeMethod = textType.GetMethod("OnAwake", BindingFlags.NonPublic | BindingFlags.Instance);
				if (onAwakeMethod != null)
				{
					var postfix = typeof(UIPatches).GetMethod(nameof(UIPatches.TextOnAwakePostfix), BindingFlags.Static | BindingFlags.Public);
					harmony.Patch(onAwakeMethod, postfix: new HarmonyMethod(postfix));
					Msg("Successfully patched Text.OnAwake");
				}
				
				Msg("Applied UI component patches");
			}
			catch (Exception ex)
			{
				Error($"Error in UI patching: {ex.Message}");
			}
		}
		
		private void AnalyzeTextComponent()
		{
			try
			{
				var textType = typeof(Text);
				Msg($"=== TEXT COMPONENT ANALYSIS ===");
				Msg($"Full name: {textType.FullName}");
				Msg($"Assembly: {textType.Assembly.FullName}");
				Msg($"Base type: {textType.BaseType?.FullName}");
				
				// Analyze properties
				Msg($"\n=== PROPERTIES ===");
				var properties = textType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var prop in properties)
				{
					Msg($"{prop.Name}: {prop.PropertyType.Name} - CanRead: {prop.CanRead}, CanWrite: {prop.CanWrite}");
					if (prop.Name == "Content")
					{
						Msg($"  Content property type: {prop.PropertyType.FullName}");
						Msg($"  Content base type: {prop.PropertyType.BaseType?.FullName}");
						
						// Analyze Content property's Value property
						var contentValueProp = prop.PropertyType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
						if (contentValueProp != null)
						{
							Msg($"  Content.Value property: {contentValueProp.PropertyType.FullName}");
							Msg($"  Content.Value setter: {contentValueProp.GetSetMethod()?.Name}");
						}
					}
				}
				
				// Analyze methods
				Msg($"\n=== METHODS ===");
				var methods = textType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(m => m.Name.Contains("Awake") || m.Name.Contains("Start") || m.Name.Contains("Update") || m.Name.Contains("Attach") || m.Name.Contains("Init"))
					.OrderBy(m => m.Name);
				
				foreach (var method in methods)
				{
					Msg($"{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
				}
				
				// Analyze Content field structure if it exists
				var contentField = textType.GetField("Content", BindingFlags.Public | BindingFlags.Instance);
				if (contentField != null)
				{
					Msg($"\n=== CONTENT FIELD ===");
					Msg($"Content field type: {contentField.FieldType.FullName}");
				}
				
				Msg($"=== END ANALYSIS ===");
			}
			catch (Exception ex)
			{
				Error($"Error analyzing Text component: {ex.Message}");
			}
		}

		private static Dictionary<string, string> _textContentCache = new Dictionary<string, string>();
		
		// NeosVR ⇔ Resonite ProtoFlux対応表
		private static readonly Dictionary<string, string> _resoniteToNeosMap = new Dictionary<string, string>
		{
			// 数値演算
			{ "Add", "+" },
			{ "AddMulti", "+" },
			{ "Sub", "-" },
			{ "SubMulti", "-" },
			{ "Mul", "×" },
			{ "MulMulti", "×" },
			{ "Div", "÷" },
			{ "ValueMod", "%" },
			
			// 比較演算
			{ "Equals", "==" },
			{ "NotEquals", "!=" },
			{ "LessThan", "<" },
			{ "LessOrEqual", "≤" },
			{ "GreaterThan", ">" },
			{ "GreaterOrEqual", "≥" },
			{ "Approximately", "≈" },
			{ "ApproximatelyNot", "!≈" },
			
			// 論理演算
			{ "AND", "&" },
			{ "OR", "|" },
			{ "OR_Multi", "|" },
			{ "XOR", "^" },
			{ "NOT", "!" },
			
			// 数値変換
			{ "ValueSquare", "x²" },
			{ "ValueCube", "x³" },
			{ "ValueReciprocal", "1/x" },
			{ "Inverse", "A⁻¹" },
			{ "ValueNegate", "-n" },
			{ "ValueOneMinus", "1-x" },
			{ "ValuePlusMinus", "+/-" },
			{ "Magnitude", "|V|" },
			{ "SqrMagnitude", "|V|²" },
			{ "Dot", "·" },
			{ "Angle", "°" },
			{ "ValueInc", "+1" },
			{ "ValueDec", "-1" },
			
			// ビット演算
			{ "ShiftLeft", "<<" },
			{ "ShiftRight", ">>" },
			{ "RotateLeft", "ROL" },
			{ "RotateRight", "ROR" },
			
			// 制御
			{ "Conditional", "?:" },
			
			// Time系
			{ "MulDeltaTime", "×dT" },
			{ "DivDeltaTime", "÷dT" }
		};
		
		public static bool ShouldEnhanceText(Text textComponent)
		{
			if (textComponent?.Slot == null)
				return false;
			
			// Check if this text is descendant of a slot with ComponentSelector component
			if (IsDescendantOfComponentSelector(textComponent.Slot))
			{
				Msg($"Found Text as descendant of ComponentSelector: {textComponent.Content.Value}");
				return true;
			}
			
			// Alternative: Check if this text is descendant of "Node Browser" slot
			if (IsDescendantOfNodeBrowser(textComponent.Slot))
			{
				Msg($"Found Text as descendant of Node Browser: {textComponent.Content.Value}");
				return true;
			}
			
			return false;
		}
		
		private static bool IsDescendantOfComponentSelector(Slot slot)
		{
			// Check ancestors for ComponentSelector component
			Slot currentSlot = slot;
			while (currentSlot != null)
			{
				if (currentSlot.GetComponent<ComponentSelector>() != null)
				{
					Msg($"Found ComponentSelector on ancestor slot: {currentSlot.Name}");
					return true;
				}
				currentSlot = currentSlot.Parent;
			}
			return false;
		}
		
		private static bool IsDescendantOfNodeBrowser(Slot slot)
		{
			// Check ancestors for "Node Browser" slot name
			Slot currentSlot = slot;
			while (currentSlot != null)
			{
				if (currentSlot.Name == "Node Browser")
				{
					Msg($"Found Node Browser ancestor slot: {currentSlot.Name}");
					return true;
				}
				currentSlot = currentSlot.Parent;
			}
			return false;
		}
		
		private static bool IsTargetNodeText(string content)
		{
			if (string.IsNullOrEmpty(content))
				return false;
			
			// Simple check - enhance any non-empty text that is descendant of ComponentSelector or Node Browser
			// The actual filtering is done by ShouldEnhanceText method
			return !string.IsNullOrWhiteSpace(content);
		}
		
		public static string GetEnhancedTextContent(string originalContent)
		{
			if (string.IsNullOrEmpty(originalContent))
				return originalContent;
			
			if (_textContentCache.TryGetValue(originalContent, out string cachedContent))
				return cachedContent;
			
			// Check if this is a Resonite ProtoFlux node name that should be converted to NeosVR style
			if (_resoniteToNeosMap.TryGetValue(originalContent, out string neosSymbol))
			{
				// Format: NeosSymbol(ResoniteNodeName)
				string enhancedContent = $"{neosSymbol}({originalContent})";
				_textContentCache[originalContent] = enhancedContent;
				Msg($"Enhanced: {originalContent} -> {enhancedContent}");
				return enhancedContent;
			}
			
			// No enhancement needed
			_textContentCache[originalContent] = originalContent;
			return originalContent;
		}
		
		private static Type FindMatchingNodeType(string nodeName)
		{
			try
			{
				// Search for FrooxEngine types that might match this name
				var allTypes = AppDomain.CurrentDomain.GetAssemblies()
					.Where(a => a.FullName.Contains("FrooxEngine"))
					.SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
					.Where(t => !t.IsGenericTypeDefinition && 
					           !t.IsAbstract &&
					           !t.ContainsGenericParameters);
				
				// Look for exact type name matches first
				var exactMatch = allTypes.FirstOrDefault(t => t.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
				if (exactMatch != null)
					return exactMatch;
				
				// Look for ProtoFlux operator types
				var operatorTypes = allTypes.Where(t => t.Namespace?.Contains("Operators") == true);
				foreach (var type in operatorTypes)
				{
					try
					{
						// Check if this type has a NodeName property that matches
						var nodeNameProp = type.GetProperty("NodeName", BindingFlags.Public | BindingFlags.Instance);
						if (nodeNameProp != null)
						{
							// Try to get the static value (this is approximate)
							if (type.Name.Contains("RotateLeft") && nodeName == "ROL")
								return type;
							if (type.Name.Contains("And") && nodeName == "AND")
								return type;
							if (type.Name.Contains("Or") && nodeName == "OR")
								return type;
						}
					}
					catch
					{
						// Skip problematic types
					}
				}
			}
			catch
			{
				// Return null if we can't find a match
			}
			
			return null;
		}
	}
	
	public static class UIPatches
	{
		public static bool SyncStringValuePrefix(FrooxEngine.Sync<string> __instance, ref string value)
		{
			try
			{
				if (ProtoFluxNodeDisplayMod.Config?.GetValue(ProtoFluxNodeDisplayMod.enabled) != true)
					return true;
				
				// Check if this Sync<string> belongs to a Text component's Content field
				if (IsTextContentField(__instance))
				{
					// Try to enhance the text content
					string enhancedValue = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(value);
					if (enhancedValue != value)
					{
						value = enhancedValue;
						ProtoFluxNodeDisplayMod.Msg($"Enhanced text: {enhancedValue}");
					}
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in Sync<string> Value prefix: {ex.Message}");
			}
			
			return true; // Continue with original method
		}
		
		public static void TextOnAwakePostfix(Text __instance)
		{
			try
			{
				if (ProtoFluxNodeDisplayMod.Config?.GetValue(ProtoFluxNodeDisplayMod.enabled) != true)
					return;
				
				// Only process text in ProtoFlux context
				if (!ProtoFluxNodeDisplayMod.ShouldEnhanceText(__instance))
					return;
				
				string originalContent = __instance.Content.Value;
				string enhancedContent = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(originalContent);
				
				if (enhancedContent != originalContent)
				{
					__instance.Content.Value = enhancedContent;
					ProtoFluxNodeDisplayMod.Msg($"Enhanced on awake: {originalContent} -> {enhancedContent}");
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in Text OnAwake postfix: {ex.Message}");
			}
		}
		
		private static bool IsTextContentField(FrooxEngine.Sync<string> sync)
		{
			try
			{
				// Use reflection to get the parent Worker (component) from Sync field
				var workerField = sync.GetType().BaseType?.GetField("_worker", BindingFlags.NonPublic | BindingFlags.Instance);
				if (workerField != null)
				{
					var worker = workerField.GetValue(sync);
					if (worker is Text textComponent)
					{
						// Check if this sync instance is the Content field
						return ReferenceEquals(textComponent.Content, sync);
					}
				}
				
				// Alternative: check by field name if reflection fails
				var nameProperty = sync.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
				if (nameProperty != null)
				{
					var name = nameProperty.GetValue(sync) as string;
					if (name == "Content")
					{
						ProtoFluxNodeDisplayMod.Msg($"Found sync field named 'Content'");
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Msg($"Error in IsTextContentField: {ex.Message}");
			}
			return false;
		}
	}
}