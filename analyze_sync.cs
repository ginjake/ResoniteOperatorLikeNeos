using System;
using System.Reflection;
using System.Linq;
using FrooxEngine;
using FrooxEngine.UIX;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Analyzing FrooxEngine.Sync<string> structure ===");
        
        // 1. Sync<string>クラスの構造を調査
        Type syncStringType = typeof(FrooxEngine.Sync<string>);
        Console.WriteLine($"Type: {syncStringType.FullName}");
        Console.WriteLine($"Base Type: {syncStringType.BaseType?.FullName}");
        
        // 2. Valueプロパティの詳細を調査
        var valueProperty = syncStringType.GetProperty("Value");
        if (valueProperty != null)
        {
            Console.WriteLine($"\nValue Property: {valueProperty.PropertyType.Name}");
            Console.WriteLine($"Getter: {valueProperty.GetGetMethod()?.Name}");
            Console.WriteLine($"Setter: {valueProperty.GetSetMethod()?.Name}");
            Console.WriteLine($"Setter Declaring Type: {valueProperty.GetSetMethod()?.DeclaringType?.FullName}");
        }
        
        // 3. すべてのプロパティとメソッドを調査
        Console.WriteLine("\n=== All Properties ===");
        foreach (var prop in syncStringType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            Console.WriteLine($"Property: {prop.Name} ({prop.PropertyType.Name})");
            if (prop.GetSetMethod() != null)
            {
                Console.WriteLine($"  Setter: {prop.GetSetMethod().Name} in {prop.GetSetMethod().DeclaringType.Name}");
            }
        }
        
        Console.WriteLine("\n=== All Methods ===");
        foreach (var method in syncStringType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name.Contains("set_") || m.Name.Contains("Value")))
        {
            Console.WriteLine($"Method: {method.Name} in {method.DeclaringType.Name}");
            foreach (var param in method.GetParameters())
            {
                Console.WriteLine($"  Parameter: {param.Name} ({param.ParameterType.Name})");
            }
        }
        
        // 4. Text.Contentフィールドの型を調査
        Console.WriteLine("\n=== Analyzing Text.Content field ===");
        Type textType = typeof(Text);
        var contentField = textType.GetField("Content", BindingFlags.Public | BindingFlags.Instance);
        if (contentField != null)
        {
            Console.WriteLine($"Text.Content Field Type: {contentField.FieldType.FullName}");
            Console.WriteLine($"Is it Sync<string>? {contentField.FieldType == typeof(FrooxEngine.Sync<string>)}");
        }
        
        // 5. 継承階層を調査
        Console.WriteLine("\n=== Inheritance Hierarchy ===");
        Type currentType = syncStringType;
        while (currentType != null)
        {
            Console.WriteLine($"Type: {currentType.FullName}");
            
            // 各レベルでValueプロパティのsetterを確認
            var levelValueProp = currentType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (levelValueProp?.GetSetMethod() != null)
            {
                Console.WriteLine($"  Has Value setter at this level: {levelValueProp.GetSetMethod().Name}");
            }
            
            currentType = currentType.BaseType;
        }
        
        // 6. ComponentSelector関連の調査
        Console.WriteLine("\n=== ComponentSelector analysis ===");
        Type componentSelectorType = typeof(ComponentSelector);
        Console.WriteLine($"ComponentSelector type: {componentSelectorType.FullName}");
        
        // ComponentSelectorでテキスト要素がどのように作成されるかを調査
        var methods = componentSelectorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.Name.Contains("Text") || m.Name.Contains("Button") || m.Name.Contains("UI"))
            .ToArray();
            
        Console.WriteLine("ComponentSelector methods related to UI/Text:");
        foreach (var method in methods)
        {
            Console.WriteLine($"  {method.Name} (returns {method.ReturnType.Name})");
        }
    }
}