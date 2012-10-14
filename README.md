allegro
=======

Innovative open-source scripting language

Grammar draft
------

### Definition

    import (<namespace>|<var>) [from <assembly>|<file>] [as <alternate-name>]

    module <modulename>:
        <import-statements>
        <define-statements>

Module defines a pack of functions, also wraps CLR namespace into easy-memorable
package names. Namespace-wide helper functions may also defined under `module` 
statement, where interpreter will automatically read from `__global.al` below
the current working directory.

    def <method-name>[<arg-specifier>]: <statements>

Brackets can be omitted if no arguments specified.

    <arg-specifier>: ( <identifier> [:<type>| (<type1>, <type2>, ...) ] [=<default-value>], ... )

Which means type-checking can be enabled by appending `:`, then a type or a tuple of 
types. Interpreter will automatically throw TypeException if type mismatch during a 
call to type-check enabled function or CLR function.

### Expressions

    (... hold <variable-name> ...)
        <statements>
        <variable-name> = <value>

"hold" declaration can use anywhere just like variable. This statement will suspend 
current evaluation and wait until the name assigned in the *sub-scope* following it.
Interpreter will throw `CallNotFulfilledException`

    <statement> if <condition>

Syntax sugar for `if <condition>: <statement>`.

    /<regular-expression>/<params>

Similar to JavaScript's terminology, everything except `//` will be treated literally
as the argument to [System.Text.RegularExpressions.Regex][regex], while params contains 
none or more [System.Text.RegularExpressions.RegexOptions][regex.opt] flag:

* `c`: RegexOptions.Compiled
* `i`: RegexOptions.IgnoreCase
* `s`|`m`: RegexOptions.Singleline | RegexOptions.Multiline
* `w`: RegexOptions.IgnorePatternWhitespace

and so on.

[regex]: http://msdn.microsoft.com/zh-tw/library/system.text.regularexpressions.regex.aspx
[regex.opt]: http://msdn.microsoft.com/zh-tw/library/system.text.regularexpressions.regexoptions.aspx

### Lexical features

    <statement> then <statement>

Line-join, just like ` _` in VB.

    folder_path = "C:\Users\Public"
    message = 'Access denied.\r\n'

Single quote allows escape characters, while double quotes don't.

### Scripting functionalities

Some syntax is recommended for use in interactive environments, but not recommended for 
production use. These syntaxes are aimed to help speed-up on-demand scripting, and is 
encouraged to be implemented as shell language.

    def func(): return 1
    PRINT(Func())           # Gets 1 in output

Except certain cases, the interpreter is case-insensitive unless you demand a `.strict`
match at the beginning of the file.

    input = conso~.r~line()

The `~` character automatically performs greedy match of current scope. This is useful 
when you need to specify something like `ListViewVirtualItemsSelectionRangeChangedEventHandler` or so.
Base Class Library have really long names indeed.
