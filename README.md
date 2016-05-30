# Summary

Model Unity script execution order on types

# Features

- Automatically detects execution order dependencies between types and adds them to Unity's global execution order
- Ability to specify **run before** and **run after** dependency
- Detects circular dependencies

# Usage

```csharp
using UnityExecutionOrder;

[Run.Before(typeof(Script2))]
public class Script1 : MonoBehaviour {}

public class Script2 : MonoBehaviour {}

[Run.After(typeof(Script2))]
public class Script3 : MonoBehaviour {}```
