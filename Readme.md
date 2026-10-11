# Source Generator
***
* A sample project demonstrating the **incremental source generator** API available from .NET 10 / Roslyn 5.x
* Applies the **`IIncrementalGenerator`** API with `ForAttributeWithMetadataName` for efficient, IDE-friendly code generation
* Two generators are included:
  * **MethodGenerator** — apply `[AddMethods]` to any `partial` class to inject additional methods
  * **PropertyGenerator** — apply `[AddProperty]` to private fields to auto-generate public properties
***
### How it works
| Generator | Trigger | Generated output |
|---|---|---|
| MethodGenerator | `[AddMethods]` on a `partial` class | `Default()`, `LowerToUpper()`, `NextPage()`, `GeneratedMethod()` |
| PropertyGenerator | `[AddProperty]` on a private field | Public `get`/`set` property derived from the field name |

### Project structure
| Project | Target | Purpose |
|---|---|---|
| `SourceGenerator` | `netstandard2.0` | The generator assembly (loaded by the compiler) |
| `Main` | `net10.0` | Demo console app that exercises both generators |

### Notes
* The `SourceGenerator` project stays on `netstandard2.0` — required for the generator assembly to be loadable by the Roslyn compiler host regardless of the consuming project's target framework.
* `LangVersion=latest` is set on the generator project so generator _code_ can use current C# features even though the output target is `netstandard2.0`.
* To debug the generator, attach a debugger via **Debug > Attach to Process** and target the `dotnet` compiler process, or add a `Debugger.Launch()` call to the `Initialize` method temporarily.
