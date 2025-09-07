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
		public override string Version => "1.2.1";
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
				// Patch Sync<string>.Value setter
				Type syncStringType = typeof(FrooxEngine.Sync<string>);
				var valueProperty = syncStringType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
				if (valueProperty != null)
				{
					var valueSetter = valueProperty.GetSetMethod();
					if (valueSetter != null)
					{
						var prefix = typeof(UIPatches).GetMethod("SyncStringValueSetterPrefix", BindingFlags.Static | BindingFlags.Public);
						harmony.Patch(valueSetter, prefix: new HarmonyMethod(prefix));
					}
				}
				
				// Patch ComponentSelector.BuildUI method
				Type componentSelectorType = typeof(FrooxEngine.ComponentSelector);
				var buildUIMethod = componentSelectorType.GetMethod("BuildUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (buildUIMethod != null)
				{
					var postfix = typeof(UIPatches).GetMethod("BuildUIPostfix", BindingFlags.Public | BindingFlags.Static);
					harmony.Patch(buildUIMethod, postfix: new HarmonyMethod(postfix));
				}
			}
			catch (Exception ex)
			{
				Error($"Error in UI patching: {ex.Message}");
			}
		}
		

		private static Dictionary<string, string> _textContentCache = new Dictionary<string, string>();
		
		// 数値演算ノード名
		public static readonly HashSet<string> _arithmeticNodes = new HashSet<string>
		{
			"Add", "AddMulti", "Sub", "SubMulti", "ValueSubMulti", "ValueSubMulti<T>",
			"Mul", "MulMulti", "ValueMulMulti", "ValueMulMulti<T>", 
			"Div", "ValueAddMulti<T>", "ValueDivMulti<T>", "ValueMod", "ValueMod<T>"
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
			{ "ValueAddMulti<T>", "+" },
			{ "ValueDivMulti<T>", "÷" },
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
			{ "MultiAND", "&" },
			{ "MultiOR", "|" },
			{ "MultiXOR", "^" },
			
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
			{ "MultiNullCoalesce<T>", "??" },
			{ "NullCoalesce<T>", "??" },
			{ "ZeroOne", "0/1" },
			
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
					enhancedContent = $"<{neosSymbol}> {originalContent}";
				}
				else if (_comparisonNodes.Contains(originalContent))
				{
					// Format: (symbol) OriginalName
					enhancedContent = $"({neosSymbol}) {originalContent}";
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
		public static bool SyncStringValueSetterPrefix(FrooxEngine.Sync<string> __instance, ref string value)
		{
			try
			{
				if (ProtoFluxNodeDisplayMod.Config?.GetValue(ProtoFluxNodeDisplayMod.enabled) != true)
					return true;
				
				// Text.ContentのSync<string>かどうかチェック
				if (__instance.Worker is Text textComponent && __instance.Name == "Content")
				{
					if (ProtoFluxNodeDisplayMod.ShouldEnhanceText(textComponent))
					{
						if (ProtoFluxNodeDisplayMod._resoniteToNeosMap.TryGetValue(value, out string symbol))
						{
							string originalNodeName = value;
							string enhancedValue = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(value);
							
							if (enhancedValue != value)
							{
								value = enhancedValue;
								textComponent.RunInUpdates(1, () => SetButtonOrderOffset(textComponent, originalNodeName));
							}
						}
					}
				}
				
				return true; // 元の処理を継続
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in SyncStringValueSetterPrefix: {ex.Message}");
				return true;
			}
		}
		
		public static void BuildUIPostfix(FrooxEngine.ComponentSelector __instance)
		{
			try
			{
				if (ProtoFluxNodeDisplayMod.Config?.GetValue(ProtoFluxNodeDisplayMod.enabled) != true)
					return;
				
				__instance.RunInUpdates(2, () => ModifyComponentSelectorTexts(__instance));
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in BuildUIPostfix: {ex.Message}");
			}
		}
		
		private static void ModifyComponentSelectorTexts(FrooxEngine.ComponentSelector componentSelector)
		{
			try
			{
				var allTexts = componentSelector.Slot.GetComponentsInChildren<Text>();
				
				foreach (var text in allTexts)
				{
					if (text?.Content?.Value != null)
					{
						string originalText = text.Content.Value;
						
						if (ProtoFluxNodeDisplayMod._resoniteToNeosMap.TryGetValue(originalText, out string symbol))
						{
							string enhancedText = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(originalText);
							if (enhancedText != originalText)
							{
								text.Content.Value = enhancedText;
								SetButtonOrderOffset(text, originalText);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error in ModifyComponentSelectorTexts: {ex.Message}");
			}
		}
		
		private static void SetButtonOrderOffset(Text textComponent, string nodeName)
		{
			try
			{
				// Navigate to the Button slot (Text -> Slot -> Button)
				var buttonSlot = textComponent.Slot.Parent;
				if (buttonSlot?.GetComponent<FrooxEngine.UIX.Button>() != null)
				{
					if (ProtoFluxNodeDisplayMod._arithmeticNodes.Contains(nodeName))
					{
						buttonSlot.OrderOffset = 1L;
					}
					else if (ProtoFluxNodeDisplayMod._comparisonNodes.Contains(nodeName))
					{
						buttonSlot.OrderOffset = 2L;
					}
				}
			}
			catch (Exception ex)
			{
				ProtoFluxNodeDisplayMod.Error($"Error setting OrderOffset: {ex.Message}");
			}
		}
	}
}