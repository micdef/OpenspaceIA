# Coding conventions

How code and its documentation are written in OpenSpaceIA, for C#.

## Principles

1. **Intent before code.** A reader must understand the purpose and the behavior of a file, a type or a member from its header, without reading its body.
2. **Readable by a human first.** Clear code always wins over clever code. An optimization is accepted only when it is needed, measured and explained in a comment; never to save a few nanoseconds at the expense of readability.
3. **Same structure everywhere.** Every file follows the same skeleton, so that a reader always knows where to look.
4. **Documented to the last member.** Public and private members are documented. The compiler enforces it for public members (CS1591); reviews enforce it for the others.

## C#

### File header

Every file starts with a header block. It uses a plain `/* */` comment: a `/** */` or `///` comment at the top of a file is read as XML documentation by the compiler and breaks the build (CS1587).

```csharp
/*
 *	@file	src/OpenSpaceIA.Core/Agents/AgentStateService.cs
 *	@author	Defraene Michaël (AKA Sonic) (co-written by Claude)
 *
 *	@brief	Computes the state of the agents
 */
```

- `@author` names the legal author, Defraene Michaël (AKA Sonic). A file written with an AI assistant adds `(co-written by Claude)`; a file written by the author alone does not.
- The header carries no version history: the Git history is the only record of who changed what and when, so it can never be out of date.

### Usings and namespace

```csharp
// Using
using System.Text;

// Static using
// #N/A

// Named using
// #N/A

// Namespace
namespace OpenSpaceIA.Core.Agents;
```

An empty section is kept and marked `// #N/A`. The namespace is file-scoped and matches the folder.

### Regions

Every class contains the same regions, in the same order, even when they are empty. An empty region holds `// #N/A`.

```csharp
#region Members
    #region Constants
    #region Variables
    #region Delegates
#region Enumerations
#region Services
#region Accessors/Properties
    #region Members & Properties
    #region Delegates
#region Constructors
#region Destructors
#region Methods
    #region Privates
    #region Publics            (may contain thematic sub-regions)
#region Overrides
    #region Methods
    #region Operators
```

An interface contains `Enumerations`, `Accessors/Properties` (`Members`, `Delegates`) and `Methods`.

### Documentation of types and members

The `<summary>` gives the role in one sentence. The `<remarks>` give the **purpose** (why it exists) and the **behavior** (what it does, its side effects and its limits), so that the body never needs to be read to use the member.

```csharp
/// <summary>
/// Method used to add a ship to the fleet
/// </summary>
/// <remarks>
/// <para><b>Purpose:</b> registers a ship before it is placed on the board.</para>
/// <para><b>Behavior:</b> the name is checked, then refused when the fleet is full or when the name is already used.</para>
/// </remarks>
/// <param name="shipName">[Cannot be null] Name of the ship to add</param>
/// <returns>
/// True  ==> The ship has been added<br/>
/// False ==> The fleet is full or the name is already used
/// </returns>
/// <exception cref="ArgumentNullException">If the ship name is null, empty or white spaces</exception>
```

- `<summary>` starts with the kind of member: `Method used to…`, `Service used to…`, `Constructor`.
- Each `<param>` states `[Can be null]` or `[Cannot be null]` for reference types.
- A `<returns>` with several cases puts one case per line, separated by `<br/>`: in XML documentation, a plain line break is only white space and is not guaranteed to be rendered.
- One `<exception>` per thrown case.
- `<remarks>` may be omitted for trivial members (simple fields, constants, constructors that only store their parameters).
- Only tags rendered by Doxygen are used: `summary`, `remarks`, `para`, `b`, `br`, `list`, `param`, `typeparam`, `returns`, `exception`, `see`, `seealso`, `paramref`, `c`, `code`, `inheritdoc`. `<value>` and `<example>` are accepted, but Doxygen renders them as plain paragraphs, without a heading.

### Step comments

The body of a method is cut into steps, each introduced by a comment that states its intent. The last step is always `// Function's return`.

```csharp
public bool AddShip(string shipName)
{
    // Check
    if (string.IsNullOrWhiteSpace(shipName))
        throw new ArgumentNullException(nameof(shipName), $"The value of the parameter: {nameof(shipName)} cannot be null or empty");

    // Refusing a full fleet or a duplicate name
    if (_shipNames.Count >= MaxShips || _shipNames.Contains(shipName))
    {
        // Function's return
        return false;
    }

    // Adding the ship
    _shipNames.Add(shipName);

    // Function's return
    return true;
}
```

### Code rules

| Rule | Example | Enforced by |
|------|---------|-------------|
| Explicit types, never `var` | `StringBuilder text = new StringBuilder();` | Build (IDE0008) |
| Explicit object and collection creation, never `new()` nor `[ ... ]` | `new List<string> { "a" }` | Build (RCS1250) |
| Braces optional for a single-line statement, required when it spans several lines | guard clauses above | Build (IDE0011) |
| Block-bodied methods and constructors, so that each step can be commented | — | IDE suggestion |
| Classic constructors (no primary constructors), so that parameters are checked before being stored | — | IDE suggestion |
| Guard clauses first, with the parameter name and an explicit message | `throw new ArgumentNullException(nameof(x), $"…")` | Review |
| Private fields `_camelCase`; injected services `_svcName`, with the parameter `svcName`; the logger is the exception: `_logger`, with the parameter `logger` | `_svcMailing`, `svcMailing`, `_logger` | Build (fields) / review (services) |
| Interfaces `IName`, services `NameService`, asynchronous methods suffixed `Async` | `IMailingService`, `SendAsync` | Build |
| Access modifiers always explicit, no `this.` | — | Build |

Build-enforced rules are configured in [.editorconfig](../../.editorconfig): a violation fails the build.

## Tests

- A test method is named in PascalCase by the rule it proves, never with underscores (the naming rules of [.editorconfig](../../.editorconfig) refuse them): `AgentOnTwoProjectsWithoutBreakIsOverloaded`, not `IsOverloaded_TwoProjects_True`.
- Its `<summary>` states the same rule in a sentence; `<remarks>` may be omitted.
- A test of a rule receives "now" as a parameter, never the clock of the machine.
- The test data is invented, never copied from a real `~/.claude`.
