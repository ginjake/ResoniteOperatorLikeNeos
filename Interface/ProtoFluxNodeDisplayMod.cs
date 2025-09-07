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
		public override string Version => "1.2.0";
		public override string Link => "https://github.com/ginjake/ResoniteOperatorLikeNeos";
		
		public static ModConfiguration Config;
		
		[AutoRegisterConfigKey]
		public static readonly ModConfigurationKey<bool> enabled = 
			new ModConfigurationKey<bool>("enabled", "Enable enhanced node display", () => true);
		
		public override void OnEngineInit()
		{
			Msg("ProtoFlux Node Display Mod: OnEngineInit called");
			try
			{
				Config = GetConfiguration();
				Config.Save(true);
				Msg("ProtoFlux Node Display Mod: Config initialized");
				
				Harmony harmony = new Harmony("net.protoflux.nodedisplay");
				Msg("ProtoFlux Node Display Mod: Harmony created");
				
				// Apply UI component patches
				ApplyUIPatches(harmony);
				Msg("ProtoFlux Node Display Mod: Initialization completed");
			}
			catch (Exception ex)
			{
				Error($"Failed to initialize ProtoFlux Node Display Mod: {ex}");
				Error($"Stack trace: {ex.StackTrace}");
			}
		}
		
		private void ApplyUIPatches(Harmony harmony)
		{
			try
			{
				// Text.Content.Valueのsetterをパッチ
				Type syncStringType = typeof(FrooxEngine.Sync<string>);
				var valueProperty = syncStringType.GetProperty("Value");
				var setMethod = valueProperty?.GetSetMethod();
				
				Msg($"Sync<string>.Value setter found: {setMethod != null}");
				
				if (setMethod != null)
				{
					var prefixMethod = typeof(UIPatches).GetMethod("SyncStringValueSetterPrefix", BindingFlags.Public | BindingFlags.Static);
					
					harmony.Patch(
						setMethod,
						prefix: new HarmonyMethod(prefixMethod)
					);
					
					Msg("Sync<string>.Value setter patched successfully");
				}
				
				// ComponentSelector.BuildUIメソッドにパッチを追加
				TryBuildUIPatches(harmony);
				
				// 代替アプローチも試す
				TryAlternativePatches(harmony);
			}
			catch (Exception ex)
			{
				Error($"Error in UI patching: {ex.Message}");
				Error($"Stack trace: {ex.StackTrace}");
			}
		}
		
		private void TryBuildUIPatches(Harmony harmony)
		{
			try
			{
				Type componentSelectorType = typeof(FrooxEngine.ComponentSelector);
				var buildUIMethod = componentSelectorType.GetMethod("BuildUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				
				if (buildUIMethod != null)
				{
					Msg($"BuildUI method found: {buildUIMethod.Name}");
					
					var postfixMethod = typeof(UIPatches).GetMethod("BuildUIPostfix", BindingFlags.Public | BindingFlags.Static);
					
					harmony.Patch(
						buildUIMethod,
						postfix: new HarmonyMethod(postfixMethod)
					);
					
					Msg("ComponentSelector.BuildUI patched successfully");
				}
				else
				{
					Msg("BuildUI method not found");
				}
			}
			catch (Exception ex)
			{
				Error($"Error in TryBuildUIPatches: {ex.Message}");
			}
		}
		
		private void TryAlternativePatches(Harmony harmony)
		{
			try
			{
				// 代替1: ComponentSelectorのUI構築メソッドを探す
				Type componentSelectorType = typeof(FrooxEngine.ComponentSelector);
				var methods = componentSelectorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				
				Msg($"ComponentSelector methods found: {methods.Length}");
				foreach (var method in methods.Take(10))
				{
					Msg($"Method: {method.Name}, ReturnType: {method.ReturnType.Name}");
				}
				
				// 代替2: Text型のメソッドを調査
				Type textType = typeof(Text);
				var textMethods = textType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(m => m.Name.Contains("Content") || m.Name.Contains("Text") || m.Name.Contains("Update"))
					.Take(5);
				
				Msg("Text component methods:");
				foreach (var method in textMethods)
				{
					Msg($"Text method: {method.Name}");
				}
				
				// 代替3: より広範囲なSync<T>パッチを試す
				TryGenericSyncPatch(harmony);
			}
			catch (Exception ex)
			{
				Error($"Error in TryAlternativePatches: {ex.Message}");
			}
		}
		
		private void TryGenericSyncPatch(Harmony harmony)
		{
			try
			{
				// ComponentSelectorの実装を詳しく調査
				Type componentSelectorType = typeof(FrooxEngine.ComponentSelector);
				
				// すべてのメソッドを調査
				var allMethods = componentSelectorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				Msg($"ComponentSelector total methods: {allMethods.Length}");
				
				// UI作成関連メソッドを探す
				var uiMethods = allMethods.Where(m => 
					m.Name.Contains("UI") || 
					m.Name.Contains("Build") || 
					m.Name.Contains("Create") || 
					m.Name.Contains("Update") ||
					m.Name.Contains("Generate") ||
					m.Name.Contains("Setup") ||
					m.Name.Contains("Text")).ToArray();
				
				Msg($"UI-related methods found: {uiMethods.Length}");
				foreach (var method in uiMethods.Take(10))
				{
					Msg($"UI Method: {method.Name}, Parameters: {method.GetParameters().Length}");
				}
				
				// ProtoFlux関連クラスも調査
				TryProtoFluxAnalysis();
			}
			catch (Exception ex)
			{
				Error($"Error in TryGenericSyncPatch: {ex.Message}");
			}
		}
		
		private void TryProtoFluxAnalysis()
		{
			try
			{
				// ProtoFlux関連アセンブリからノードブラウザ関連クラスを探す
				var protoFluxAssembly = AppDomain.CurrentDomain.GetAssemblies()
					.FirstOrDefault(a => a.FullName.Contains("ProtoFlux"));
				
				if (protoFluxAssembly != null)
				{
					Msg($"ProtoFlux assembly found: {protoFluxAssembly.FullName}");
					
					var nodeTypes = protoFluxAssembly.GetTypes()
						.Where(t => t.Name.Contains("Node") || t.Name.Contains("Browser") || t.Name.Contains("Selector"))
						.Take(10);
					
					foreach (var nodeType in nodeTypes)
					{
						Msg($"ProtoFlux type: {nodeType.FullName}");
					}
				}
				
				// テキスト設定の代替パターンを探す
				TryAlternativeTextPatches();
			}
			catch (Exception ex)
			{
				Error($"Error in TryProtoFluxAnalysis: {ex.Message}");
			}
		}
		
		private void TryAlternativeTextPatches()
		{
			try
			{
				// ButtonEventHandler`1[System.String]を探す（ログで見つかった）
				Type buttonEventHandlerType = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
					.FirstOrDefault(t => t.Name == "ButtonEventHandler" && t.IsGenericType);
				
				if (buttonEventHandlerType != null)
				{
					Msg($"ButtonEventHandler found: {buttonEventHandlerType.FullName}");
					
					// ButtonEventHandler<string>のイベント/メソッドを調査
					var stringButtonEvent = buttonEventHandlerType.MakeGenericType(typeof(string));
					var eventMethods = stringButtonEvent.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						.Where(m => m.Name.Contains("Invoke") || m.Name.Contains("Event"))
						.Take(5);
					
					foreach (var method in eventMethods)
					{
						Msg($"ButtonEvent method: {method.Name}");
					}
				}
				
				// ProtoFluxノードの実際の実装を調査
				TryProtoFluxNodeAnalysis();
				
				// LocaleStringDriverの調査
				Type localeDriverType = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
					.FirstOrDefault(t => t.Name == "LocaleStringDriver");
				
				if (localeDriverType != null)
				{
					Msg($"LocaleStringDriver found: {localeDriverType.FullName}");
				}
			}
			catch (Exception ex)
			{
				Error($"Error in TryAlternativeTextPatches: {ex.Message}");
			}
		}
		
		private void TryProtoFluxNodeAnalysis()
		{
			try
			{
				// 数値演算ノード（Add）の実際の実装を調査
				Type addNodeType = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
					.FirstOrDefault(t => t.Name == "Add" && t.Namespace != null && t.Namespace.Contains("ProtoFlux"));
				
				if (addNodeType != null)
				{
					Msg($"Add node type found: {addNodeType.FullName}");
					
					// NodeNameプロパティの実装を確認
					var nodeNameProp = addNodeType.GetProperty("NodeName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
					if (nodeNameProp != null)
					{
						Msg($"NodeName property found: {nodeNameProp.PropertyType.Name}");
					}
					
					// GetterとSetterの確認
					var getter = nodeNameProp?.GetGetMethod();
					var setter = nodeNameProp?.GetSetMethod();
					Msg($"NodeName getter: {getter != null}, setter: {setter != null}");
				}
				
				// ProtoFluxノードの基底クラスを調査
				Type protoFluxNodeType = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
					.FirstOrDefault(t => t.Name.Contains("ProtoFluxNode") || (t.Name == "ProtoFluxNode"));
				
				if (protoFluxNodeType != null)
				{
					Msg($"ProtoFluxNode base type found: {protoFluxNodeType.FullName}");
				}
			}
			catch (Exception ex)
			{
				Error($"Error in TryProtoFluxNodeAnalysis: {ex.Message}");
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
					ProtoFluxNodeDisplayMod.Msg($"Text.Content detected: '{value}', ComponentSelector: {ProtoFluxNodeDisplayMod.IsDescendantOfComponentSelector(textComponent.Slot)}");
					
					if (ProtoFluxNodeDisplayMod.ShouldEnhanceText(textComponent))
					{
						ProtoFluxNodeDisplayMod.Msg($"ShouldEnhance: true for '{value}'");
						
						if (ProtoFluxNodeDisplayMod._resoniteToNeosMap.TryGetValue(value, out string symbol))
						{
							string originalNodeName = value; // 変換前の名前を保存
							string enhancedValue = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(value);
							ProtoFluxNodeDisplayMod.Msg($"Converting: '{value}' -> '{enhancedValue}'");
							
							if (enhancedValue != value)
							{
								value = enhancedValue;
								
								// OrderOffsetを設定（次回の更新で実行、元のノード名を使用）
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
				
				ProtoFluxNodeDisplayMod.Msg("BuildUI postfix called - modifying ProtoFlux node texts");
				
				// BuildUI完了後に少し遅延してテキストを変更
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
				ProtoFluxNodeDisplayMod.Msg("Modifying ComponentSelector texts after BuildUI");
				
				// ComponentSelector内のすべてのTextコンポーネントを取得
				var allTexts = componentSelector.Slot.GetComponentsInChildren<Text>();
				int modifiedCount = 0;
				
				foreach (var text in allTexts)
				{
					if (text?.Content?.Value != null)
					{
						string originalText = text.Content.Value;
						
						// ProtoFluxノード名の変換をチェック
						if (ProtoFluxNodeDisplayMod._resoniteToNeosMap.TryGetValue(originalText, out string symbol))
						{
							string enhancedText = ProtoFluxNodeDisplayMod.GetEnhancedTextContent(originalText);
							if (enhancedText != originalText)
							{
								text.Content.Value = enhancedText;
								ProtoFluxNodeDisplayMod.Msg($"Modified text: '{originalText}' -> '{enhancedText}'");
								modifiedCount++;
								
								// OrderOffsetを設定
								SetButtonOrderOffset(text, originalText);
							}
						}
					}
				}
				
				ProtoFluxNodeDisplayMod.Msg($"Total texts modified: {modifiedCount}");
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