# *JAML = WPF XAML − XML.verbosity + JSON.elegance*

https://github.com/Athari/Alba.Jaml

WARNING
=======

This library is *NOT* ready to be used in real projects. It's merely a **proof-of-concept**. If it gains substantial interest, I'll work on it more. Currently I need to make tough choices in architecture (see *Open questions* section below), and any input will be valuable.

Features
========

* JSON syntax for WPF instead of built-in XML syntax.
* Short and concise syntax for built-in markup extensions: you'll never write `{Binding Path=Name, RelativeSource={RelativeSource AncestorType={x:Type Button}}, Converter={StaticResource Converter}}` again (it'll be `{= ~Button.Name, Converter={@Converter} }`).
* Inline C# expressions for bindings and multi-bindings, implemented as auto-generated converters: you'll never write `FirstValueEqualsToSecondOrThirdValueEqualsNullConverter` (it'll be `{= ${=Property1} == ${=Property2} || ${=Property3} == null }`). Works with properties, style setters, triggers — anywhere where bindings can be used.
* Simple syntax for "element" properties: you'll never write `<Setter.Value>` again.
* Types are inferred automatically: you'll never repeat `ColumnDefinition` for each column again.
* Simple setters and triggers syntax: you'll never painfully convert properties to setters again (they'll have the same syntax), you'll never write ten levels deep multi-triggers with multi-bindings again.
* (TODO) C#-style `using` directives.

Example
=======

```javascript
_={
    $: 'Window root',
    Resources: [{
        $: 'Style MyButtonStyle Button',
        set: {
            Background: 'Red', Foreground: 'Green'
        },
        on: {
            '{=this.IsMouseOver}': {set: {
                Background: 'Yellow', Foreground: 'Blue'
            }}
        }
    }],
    _: [{
        $: 'Grid',
        RowDefinitions: [ { Height: '*' } ],
        ColumnDefinitions: [ { Width: '*' } ],
        _: [{
            $: 'Button btnPressMe', Content: 'Press me!', Style: '{@MyButtonStyle}'
        }]
    }]
}
```

Current limitations
===================

* Only built-in types from PresentationCore and PresentationFramework assemblies are supported.
* Error reporting is almost non-existent. If you write something wrong, you'll have to figure out what went wrong yourself (probably by commenting parts of the code).
* Numerous bugs. The library is written to prove it can be written, not to actually develop with it.

Design priorites
================

1. Functionality
2. Detailed error reporting
3. What can be determined automatically, must be determined automatically
4. Short and conscise syntax
5. Readable and easy to remember syntax
6. Effeciency and optimization
7. Generated XAML readability

Documentation
=============

See wiki:

[Getting started](https://github.com/Athari/Alba.Jaml/wiki/Getting-started)

[JAML Syntax](https://github.com/Athari/Alba.Jaml/wiki/JAML-Syntax)

[Example 1: Simple window](https://github.com/Athari/Alba.Jaml/wiki/Example-1:-Simple-window)

[Example 2: TreeViewItem template](https://github.com/Athari/Alba.Jaml/wiki/Example-2:-TreeViewItem-template)

To-do list
==========

* Use just one type reflection mechanism (currently Reflection and XamlType-based). Also see *Open questions* below.
* Detailed error reporting with line and character numbers.
* Convert `on:[{ '{=this.Prop}': {...}` to `Trigger`, not `DataTrigger` with `Binding RelativeSource=Self`.
* More sensible XAML formatting (reach maximum line length, wrap only then?).
* Automatically detect types for Templates within styles etc.
* Detect types of multi-binding's sub-bindings in expressions, if possible.

Open questions
==============

* Evaluate posibility of a **new language**.
  * _Alternative 1:_ extend JSON. The best option is probably to modify [Json.NET][].
    * Allow to specify type and name and possibly visibility of an object (either in front of `{`, like in [QML][], or after, like in markup extensions), to avoid `$` property.
    * Allow to put any tokens into objects, not just properties, to avoid `_` property.
    * Allow any characters, except special characters, to be part of names and identifiers. Avoid unnecessary quotes.
    * Make markup extensions first-class citizens in JAML. No more strings, make them true parsed objects.
  * _Alternative 2:_ use on existing script languages like Python, Lua etc.
* Solve the problem of **referencing types in an assembly which contains JAML file**. Also make `using` directive in JAML closer to C#'s `using`.
  * _Alternative 1:_ completely rely on public XAML parsing capabilities (using reflection) and make developers put JAML files into a separate project.
  * _Alternative 2:_ write alternative "reflectors" on source code. The best option is probably to use [NRefactory][].
  * _Alternative 3:_ use [Microsoft Roslyn][Roslyn] (currently CTP), whatever it is.

License
=======
**TL;DR: Simplified BSD License**

Copyright (c) 2012, Alexander Prokhorov
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ALEXANDER PROKHOROV BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

Links
=====

The library uses the following third-party code:
* [**Json.NET** by James Newton-King](http://json.net/), JSON library for .NET.
* [**T4MultiFile** by Brandon D'Imperio](http://nuget.org/packages/T4MultiFile), T4 helper file to help generate multiple file outputs.
* `MS.Internal` namespace of `System.Xaml` from .NET Framework, partially extracted with [.NET Reflector][Reflector].

[Json.NET]: http://json.net/ "Json.NET by James Newton-King, JSON library for .NET"
[QML]: http://doc.qt.digia.com/qt/qtquick.html "Qt Modeling Language, part of Qt Quick, part of Qt Framework"
[NRefactory]: https://github.com/icsharpcode/NRefactory/ "C# analysis library used in the SharpDevelop and MonoDevelop IDEs"
[Roslyn]: http://msdn.microsoft.com/en-us/roslyn "APIs for exposing the Microsoft C# and Visual Basic .NET compilers as services"
[Reflector]: http://www.reflector.net/ "The tool for decompiling .NET assemblies"
