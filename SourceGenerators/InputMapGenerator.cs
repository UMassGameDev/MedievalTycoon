using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourceGenerators;

/// <summary>
/// This is a source generator that gives us a strongly typed InputMapHandler based on the input actions defined in project.godot.
/// Dope shit right?\
///
/// I dunno if theres better ways to do source generators and I no longer have the guide I used for this one so GL understanding it.
/// </summary>
[Generator]
public class InputMapGenerator : IIncrementalGenerator {
    private const string InputMapHandlerFileName = "InputMapHandler.generated.cs";
    private const string InputActionFileName = "InputAction.generated.cs";
    private const string InputActionClass = """
                                            using Godot;
                                            using System;
                                            
                                            namespace Godot;

                                            public class InputAction(StringName action) {
                                                public StringName Action => action;

                                                public bool IsPressed => Input.IsActionPressed(action);
                                                public bool IsJustPressed => Input.IsActionJustPressed(action);
                                                public bool IsJustReleased => Input.IsActionJustReleased(action);
                                                public float Strength => Input.GetActionStrength(action);

                                                public event Action Pressed;
                                                public void InvokePressed() => Pressed?.Invoke();
                                                public void Press() {
                                                    Input.ActionPress(action);
                                                    InvokePressed();
                                                }

                                                public event Action Released;
                                                public void InvokeReleased() => Released?.Invoke();
                                                public void Release() {
                                                    Input.ActionRelease(action);
                                                    InvokeReleased();
                                                }
                                            }
                                            """;
    
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(ctx => {
            var sourceText = SourceText.From(InputActionClass, Encoding.UTF8);
            ctx.AddSource(InputActionFileName, sourceText);
        });

        IncrementalValuesProvider<AdditionalText> additionalFiles = context.AdditionalTextsProvider.Where(file => Path.GetFileName(file.Path) == "project.godot");
        context.RegisterSourceOutput(additionalFiles.Collect(), (ctx, files) => {
            if (files.Length == 0) { return; }

            AdditionalText? projectGodotFile = files.FirstOrDefault();
            SourceText? content = projectGodotFile?.GetText();
            if (content == null) { return; }
            
            string[] inputActions = GetInputActions(content);
            
            StringBuilder inputMapHandler = new StringBuilder();
            inputMapHandler.AppendLine("using Godot;");
            inputMapHandler.AppendLine();
            inputMapHandler.AppendLine("namespace Godot;");
            inputMapHandler.AppendLine();
            inputMapHandler.AppendLine("public partial class InputMapHandler : Node {");
            inputMapHandler.AppendLine("    public void HandleInputMap(InputEvent @event) {");
            foreach (string action in inputActions) {
                inputMapHandler.AppendLine($"        if (@event.IsActionPressed({action}.Action)) {{ {action}.InvokePressed(); }}");
                inputMapHandler.AppendLine($"        if (@event.IsActionReleased({action}.Action)) {{ {action}.InvokeReleased(); }}");
            }
            inputMapHandler.AppendLine("    }");
            inputMapHandler.AppendLine();
            foreach (string action in inputActions) {
                inputMapHandler.AppendLine($"    public static readonly InputAction {action} = new InputAction(\"{action}\");");
            }
            inputMapHandler.AppendLine("}");
            ctx.AddSource(InputMapHandlerFileName, SourceText.From(inputMapHandler.ToString(), Encoding.UTF8));
        });
    }
    
    private static string[] GetInputActions(SourceText godotProjectFile) {
        List<string> actions = [];
        int index = 0;
        for (; index < godotProjectFile.Lines.Count; index++) {
            string text = godotProjectFile.Lines[index].ToString().Trim();
            if (text == "[input]") { break; }
        }
        index++;
        for (; index < godotProjectFile.Lines.Count; index++) {
            string text = godotProjectFile.Lines[index].ToString().Trim();
            if (string.IsNullOrEmpty(text)) { continue; }
            if (text.StartsWith("[")) { break; }
            if (!TryMatchInput(text, out string? action)) { continue; }
            action = action!.Replace('.', '_');
            actions.Add(action);
        }

        return actions.ToArray();
    }

    private static readonly Regex InputRegex = new Regex(@"^(?<Input>[^=]+)=.*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    private static bool TryMatchInput(string line, out string? action) {
        var match = InputRegex.Match(line);
        if (match.Success) {
            action = match.Groups["Input"].Value;
            return true;
        }
        action = null;
        return false;
    }
}