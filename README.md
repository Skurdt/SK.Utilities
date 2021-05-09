# Unity package: SK.Utilities
Various C# extensions and utilities

## [Algorithms](Runtime/Algorithms/Algorithms.cs):
- Swap
```cs
public static void Swap<T>(ref T lhs, ref T rhs);
```

## [Array Extensions](Runtime/Array/ArrayExtensions.cs):
- Rotate
```cs
public static void RotateLeft<T>(this T[] arr, int count);
public static void RotateRight<T>(this T[] arr, int count);
```
Shift all elements _count_ number of times.</br>
Left means moving the first element(s) to the end.</br>
Right means moving the last elements(s) to the begining.

## [DynamicLibrary](Runtime/DynamicLibrary):
A wrapper around "LoadLibraryA" for Windows and "dlopen" for UNIX for loading, retrieving function pointers and freeing native dynamically linked libraries.

## [Ini](Runtime/Ini/IniFile.cs):
I can't find the original sources... Modified to allow for parsing keys-only ini files.

## [List Extensions](Runtime/List/ListExtensions.cs):
- AddStringIfNotNullOrEmpty
```cs
public static void AddStringIfNotNullOrEmpty(this List<string> list, string toAdd);
```
- Rotate
```cs
public static void RotateLeft<T>(this List<T> list, int count = 1);
public static void RotateRight<T>(this List<T> list, int count = 1);
```
Same as the array extensions.

## [Math](Runtime/Math/MathUtils.cs):
- Clamp
```cs
public static T Clamp<T>(this T val, T min, T max);
```

## [OS](Runtime/OS/OSUtils.cs):
- ProcessLauncher
```cs
public sealed class ProcessLauncher;
```
A wrapper around C# [Process](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=netstandard-2.0) where it's possible to pass custom "logger" functions (invoked by both this class, STDOUT and STDERR).

## [StateMachine](Runtime/StateMachine):
```cs
public abstract class Context<T>;
public abstract class State<T>;
```
A generic state machine.</br>
- User must implement a non-generic and non-abstract class that derives from Context\<T> and a non-generic and abstract class that derives from State\<T>.
- The derived context can inject any external dependencies through a constructor.
- All operations inside the states can only be done through the context, as such, the derived abstract state and all concrete states should add the concrete context as a protected member, assigned via a constructor.
- States are not created using "new" from the caller side. They are created by the context using [System.Activator.CreateInstance](https://docs.microsoft.com/en-us/dotnet/api/system.activator.createinstance?view=netstandard-2.0) when calling "TransitionTo" and are cached internally.

**Example usage:**
```cs
public sealed class PlayerContext : SK.Utilities.StateMachine.Context<PlayerState>
{
    public void SayHello() => Console.WriteLine("Hello!");

    public void SayGoodbye() => Console.WriteLine("Goodbye!");
}

public abstract class PlayerState : SK.Utilities.StateMachine.State<PlayerState>
{
    protected PlayerContext _context;

    public PlayerState(PlayerContext context) => _context = context;
}

public sealed class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerContext context)
    : base(context)
    {
    }
  
    public override void OnEnter() => _context.SayHello();

    public override void OnExit() => _context.SayGoodbye();
}

public static void Main()
{
    PlayerContext playerContext = new PlayerContext();
    playerContext.TransitionTo<PlayerIdleState>();
}
```

## [Strings](Runtime/Strings/StringUtils.cs):
- ValueOrDefault
```cs
public static string ValueOrDefault(this string @string, string @default = null);
```

## [Unsafe Strings](Runtime/Strings/UnsafeStringUtils.cs):
- StringToChars
```cs
public static char* StringToChars(string src, out IntPtr ptr);
```
Use the out parameter to free it later.

- CharsToString
```cs
public static string CharsToString(char* str);
```

## [XML](Runtime/XML/XMLUtils.cs):
- Serialize
```cs
public static bool Serialize<T>(string filePath, T value);
```

- Deserialize
```cs
public static T Deserialize<T>(string filePath);
public static async Task<T> DeserializeAsync<T>(string filePath);
```
