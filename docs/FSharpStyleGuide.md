# F# Coding Style

* We use PascalCase for namespaces, types, methods, properties and record
members, but camelCase for parameters, private fields, and local functions
(in unit tests we have the exception of allowing under_score_naming for
fields in cases where it improves readability).
* When writing non-static type members we prefer to use the word `self`.
* We follow the same convention of C# to prefix interfaces with the uppercase
letter 'I'.
* Given that we use the C#ish style of PascalCase for type names (instead of
camelCase), then it only makes sense to try to use the type names which start
with uppercase, instead of the camelCased F# types (e.g. use `Option` and `List`
instead of `option` and `list`). The only exception to this rule is: primitive
types (where we prefer `string` and `int` over `String` and `Int32` unless we're
using a static method of them; and `array` over `Array` because they are actually
different things).
* To not confuse array types with lists, we prefer to use `List.Empty` over `[]`
(where it's possible; e.g. in match cases it's not possible), and `array<Foo>`
over `Foo []`.
* To not confuse lists/arrays of one element with indexers, we prefer to use the
`singleton` function of the List/Array/Seq modules, instead of the less verbose
(and easy to misinterpret, e.g. think it's a list when it's an array, or
viceversa): `[ oneItem ]` or `[| oneItem |]`. (If inside a match case, then
rather use `oneItem::List.Empty` instead of `[oneItem]`.)
* We prefer the generic notation `Foo<Bar>` rather than `Bar Foo` (see
https://github.com/fsprojects/fantomas/issues/712 ).
* We prefer to not use the shadowing practice, even if the F# compiler allows it
(not to confuse shadowing with mutation, which is also discouraged anyway).
* We prefer to write parentheses only when strictly necessary (e.g. in F# they
are not required for `if` clauses, unlike C#) or for readability purposes (e.g.
when it's not clear what operator would be applied first, as not everyone knows
the rules of the language for default operator precedence by heart).
* Whenever possible, we prefer to use currified arguments (instead of tuples),
should we need to use F# partial application.
* We avoid writing the keyword `new` for instances of non-IDisposable types.
* When dealing with `Option<Foo>` elements, we consider it's much safer to use
`match` patterns (or the functions `Option.iter` and `Option.exists`) instead
of using the less safe approaches  `x.IsSome && x.Value = ...` or
`x.IsNone || x.Value = ...`, which might break easily when refactoring them.
* In case of doubt, we prefer to expliticly add the accessibility keywords
(`private`, `public`, `internal`...), should the F# language allow it.
* With `if` blocks we prefer to put the `then` keyword in the same line as the
`if`, but use a newline afterwards; and the `else` or `elif` keywords indented
to be aligned with the `if`. Example:

```
if foo.SomeBoolProperty then
    DoSomething()
elif foo.SomeFuncReturingBool() then
    DoOtherThing()
else
    DoYetAnotherThing()
```

Another example:

```
let someVariableToBeAssigned =
    if foo.SomeBoolProperty then
        "someValue"
    elif foo.SomeOtherCondition() then
        "otherValue"
    else
        "elseValue"
```

* A space should be added after the colon (and not before) when denoting a type,
so: `(foo: Foo)`
* When using property initializers, we prefer to use the immutable syntax sugar:
```
let foo = Foo(Bar = bar, Baz = baz)
```
instead of the more verbose (and scary)
```
let foo = Foo()
foo.Bar <- bar
foo.Baz <- baz
```
* When laying out XamarinForms UIs, we prefer to use XAML (if possible) instead
of adding them programmatically with code.
* The `open` keyword should be used to open namespaces if and only if the
element used from it is used more than once in the same file.
* The `open` statements should be grouped in three buckets: the first for the
namespaces that come from .NET base class libraries, the second one for external
libraries (e.g. nugets), and the last for the namespaces that belong to the
source code that lives in this repository.
* We prefer the short F# syntax to declare exception types (just
`exception Foo of Bar*Baz`) except when constructors need to be used (e.g. for
passing the inner exception to the base class).
* We only use the `mutable` keyword when strictly necessary. Should you need it,
special precautions should be taken to access the element from one exclusive
thread (e.g. by using locks). In order to write immutable algorithms (as opposed
to imperative-style ones), should you need to write recursive functions to
compose them, you have to make sure they are tail-recursive-friendly, to not
cause stack-overflow exceptions.
* When creating Tasks in UI code (Xamarin.Forms), don't run them without some
careful guarding (e.g. we want to fail fast, as in crash the app, if any
exception happens in it); for example, you could use the special function
`FrontendHelpers.DoubleCheckCompletion` to help on this endeavour.
* Don't use abbreviations or very short names on variables, types, methods, etc.
We prefer to be verbose and readable than compact and clever.
* Don't over-comment the code; splitting big chunks of code into smaller
functions with understandable names is better than adding comments that may
become obsolete as the code evolves.
* We prefer the Java way of mapping project names and namespaces with the tree
structure of the code. For example, a module whose full name is Foo.Bar.Baz
should either live in a project called "Foo.Bar" (and be named "Baz" under
the namespace "Foo.Bar"), or: in a project called "Foo", but in a subdirectory
called "Bar" (and be named "Baz" under the namespace "Foo.Bar").
* We prefer records over tuples, especially when being part of other type
structures.
* As a naming convention, variables with `Async<'T>` type should be suffixed
with `Job`, and variables with `Task<'T>` should be suffixed with `Task`.
* When adding NUnit tests, don't use `[<Test>]let Foo` and naked `module Bar`
syntax, but `[<Test>]member __.Foo` and `[<TestFixture>]type Bar()` (note the
parentheses, as it's an important bit), otherwise the tests might not run in
all platforms.
* When dealing with exceptions in async{} code, we prefer normal try-with
blocks instead of using `Async.Catch`, because the latter incentivizes the
developer to use a type-less style of catching an exception, plus the
discriminated union used for its result is quite unreadable (`Choice1Of2`
and `Choice2Of2` don't give any clue about which one is the successful case
and which one is the exceptional one).
* We prefer `async{}` blocks better than `task{}` ones because the former is
idiomatic F#.
* When using the function `ignore`, use always the generic type (`ignore<'T>`).
* Do not use `System.ParamArray` (for variable number of arguments) as it's
easy to shoot yourself in the foot, and is not idiomatic F# (it was meant for
C#). More info: https://sidburn.github.io/blog/2017/03/13/variable-arguments
* We prefer to deconstruct tuples early instead of using the ugly `fst` and
`snd` functions.
* Do not use type extensions (similar practice as monkey-patching in Python
or extension methods in C#) because:
  - It's not easy to see if the method belongs to the original type or is
extended in our code (too much magic). This can be mitigated by having a
good IDE that allows you to easily navigate with a "Go to definition" action,
but still requires explicit use of this feature instead of marking the method
visually as special compared to non-extended methods.
  - It encourages a culture of fixing bugs locally instead of contributing
fixes upstream.
