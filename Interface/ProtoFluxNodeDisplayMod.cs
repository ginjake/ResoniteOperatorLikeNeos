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
		public override string Author => "ginjake";
		public override string Version => "1.1.0";
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
				// Patch Sync<string>.Value setter to catch when text content is actually set
				var syncStringType = typeof(FrooxEngine.Sync<string>);
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
		
		// 数値演算ノード名
		public static readonly HashSet<string> _arithmeticNodes = new HashSet<string>
		{
			"Add", "AddMulti", "Sub", "SubMulti", "ValueSubMulti", "ValueSubMulti<T>",
			"Mul", "MulMulti", "ValueMulMulti", "ValueMulMulti<T>", 
			"Div", "ValueMod", "ValueMod<T>"
		};
		
		// 比較演算ノード名
		public static readonly HashSet<string> _comparisonNodes = new HashSet<string>
		{
			"Equals", "NotEquals", "LessThan", "LessOrEqual", 
			"GreaterThan", "GreaterOrEqual", "Approximately", "ApproximatelyNot"
		};
		
		// NeosVR ⇔ Resonite ProtoFlux対応表
		public static readonly Dictionary<string, string> _resoniteToNeosMap = new Dictionary<string, string>
		{
			// 数値演算
			{ "Add", "+" },
			{ "AddMulti", "+" },
			{ "Sub", "-" },
			{ "SubMulti", "-" },
			{ "ValueSubMulti", "-" },
			{ "ValueSubMulti<T>", "-" },
			{ "Mul", "×" },
			{ "MulMulti", "×" },
			{ "ValueMulMulti", "×" },
			{ "ValueMulMulti<T>", "×" },
			{ "Div", "÷" },
			{ "ValueMod", "%" },
			{ "ValueMod<T>", "%" },
			
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
			{ "ValueSquare<T>", "x²" },
			{ "ValueCube", "x³" },
			{ "ValueCube<T>", "x³" },
			{ "ValueReciprocal", "1/x" },
			{ "ValueReciprocal<T>", "1/x" },
			{ "Inverse", "A⁻¹" },
			{ "ValueNegate", "-n" },
			{ "ValueNegate<T>", "-n" },
			{ "ValueOneMinus", "1-x" },
			{ "ValueOneMinus<T>", "1-x" },
			{ "ValuePlusMinus", "+/-" },
			{ "ValuePlusMinus<T>", "+/-" },
			{ "Magnitude", "|V|" },
			{ "SqrMagnitude", "|V|²" },
			{ "Dot", "·" },
			{ "Angle", "°" },
			{ "ValueInc", "+1" },
			{ "ValueInc<T>", "+1" },
			{ "ValueDec", "-1" },
			{ "ValueDec<T>", "-1" },
			
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
			
			// Check if parent or grandparent slot is named "Split" - if so, don't enhance
			if (textComponent.Slot.Parent?.Name == "Split" || 
			    textComponent.Slot.Parent?.Parent?.Name == "Split")
				return false;
			
			// ONLY enhance text that is descendant of a slot with ComponentSelector component
			return IsDescendantOfComponentSelector(textComponent.Slot);
		}
		
		public static bool IsDescendantOfComponentSelector(Slot slot)
		{
			// Check ancestors for ComponentSelector component
			Slot currentSlot = slot;
			while (currentSlot != null)
			{
				if (currentSlot.GetComponent<ComponentSelector>() != null)
					return true;
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
					return true;
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
				string enhancedContent;
				
				// Format based on node type
				if (_arithmeticNodes.Contains(originalContent))
				{
					// Format: <symbol> OriginalName
					enhancedContent = $"({neosSymbol}) {originalContent}";
				}
				else if (_comparisonNodes.Contains(originalContent))
				{
					// Format: (symbol) OriginalName
					enhancedContent = $"[{neosSymbol}] {originalContent}";
				}
				else
				{
					// Format: OriginalName (symbol)
					enhancedContent = $"{originalContent} ({neosSymbol})";
				}
				
				_textContentCache[originalContent] = enhancedContent;
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
				
				// Only process non-empty strings that could be node names
				if (string.IsNullOrEmpty(value) || value.Length > 30)
					return true;
				
				// Only process if we have a mapping for this value
				if (!ProtoFluxNodeDisplayMod._resoniteToNeosMap.ContainsKey(value))
					return true;
				
				// Find the Text component that owns this sync
				var textComponent = GetTextComponentFromSync(__instance);
				if (textComponent != null && ProtoFluxNodeDisplayMod.ShouldEnhanceText(textComponent))
				{
					string enhancedValue = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(value);
					if (enhancedValue != value)
					{
						value = enhancedValue;
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in SyncStringValuePrefix: {ex.Message}");
			}
			
			return true;
		}
		
		private static Text GetTextComponentFromSync(FrooxEngine.Sync<string> sync)
		{
			try
			{
				var syncType = sync.GetType();
				
				// Method 1: Check _worker field in base type
				var workerField = syncType.BaseType?.GetField("_worker", BindingFlags.NonPublic | BindingFlags.Instance);
				if (workerField != null)
				{
					var worker = workerField.GetValue(sync);
					if (worker is Text textComponent && ReferenceEquals(textComponent.Content, sync))
					{
						return textComponent;
					}
				}
				
				// Method 2: Try different field names
				var parentField = syncType.BaseType?.GetField("_parent", BindingFlags.NonPublic | BindingFlags.Instance);
				if (parentField != null)
				{
					var parent = parentField.GetValue(sync);
					if (parent is Text textComponent && ReferenceEquals(textComponent.Content, sync))
					{
						return textComponent;
					}
				}
				
				// Method 3: Search all Text components in the world for this sync (fallback)
				var world = Engine.Current?.WorldManager?.FocusedWorld;
				if (world != null)
				{
					var allTexts = world.RootSlot.GetComponentsInChildren<Text>();
					foreach (var text in allTexts)
					{
						if (ReferenceEquals(text.Content, sync))
						{
							return text;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in GetTextComponentFromSync: {ex.Message}");
			}
			return null;
		}
	}
}