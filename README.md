allegro
=======
Innovative open-source scripting language

Designed scenarios
------
* Quick but complicated task solving
* Experimenting with library functions
* Hosted environment scripting, e.g. shell
* Program shorthand during interactive sessions

Though mainly drafted with CLI Base Class Library, **allegro** is a language 
which is designed to be placed on top of any existing framework. Further 
implementation of standard libraries opens for discussion.

Grammar draft
------

### Definition

    import (<namespace>|<var>) [from <assembly>|<file>] [as <alternate-name>]

    class <type-name>[ (<interfaces-or-base-type>, ... ) ]: <definitions>

Brackets can be omitted to create a blank class with no interface implementation.

    def [<type>] <field-name>
	def [<type>] <property-name>:
		[get: <statements>][set: <statements>] | pass

Thus can easily expand a field member to an instance property.

    def [<type>] <method-name>( [<arguments>] ): <statements>
    <arguments>: [<type> | (<type1>, <type2>, ...) ] <identifier> [=<default-value>], ...

Which means type-checking can be enabled by prepending a type or a tuple of 
types. Interpreter will automatically throw TypeException if type mismatch during a 
call to type-check enabled function or CLR function.

### Expressions

    await <variable-name>

`await` declaration can be used in combination with a valid identifier anywhere just like 
normal variable. This statement will suspend the evaluation until a variable with an 
exact identifier was assigned in the current scope or sub-scope. The runtime should 
throw a `CallNotFulfilledException` if the code tries to exit the current scope 
without ever assigning the temporary variable. 

    <statement> if <condition>

Syntax sugar for `if <condition>: <statement>`.

    $/<regular-expression>/<params>

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

Line-join, just like `:` in VB.

    folder_path = "C:\Users\Public"
    message = 'Access denied.\r\n'

Single quote allows escape characters, while double quotes don't.

### Scripting functionalities

    def func(): return 1
    PRINT(Func())           # Gets 1 in output

Except certain cases, the interpreter is case-insensitive unless you demand a `.strict`
match at the beginning of the file.

    input = conso~.r~line()

The `~` character automatically performs greedy match of current scope. This is useful 
when you need to specify something like `ListViewVirtualItemsSelectionRangeChangedEventHandler` or so.
Base Class Library have really long names indeed.

Some syntax is recommended for use in interactive environments, but not recommended for 
production use since effects of these statements in a compilation scenario are undefined. 
These syntaxes are aimed to help speed-up on-demand scripting, and is encouraged to be 
implemented as shell language.

### Hosted enviroment

**allegro**, if chosen as scripting language, exposes certain features to tighten intergation
between host and client. 

    require <host-feature>

The `require` statement could be used to instruct the hosting intepreter or 
application to enable / include additional support for a requested functionality.

A hosting environment can register additional classes and/or global functions 
into current interpreter's scope, effectively enables interoped calls 
between the two. **Security model of this interaction is yet to be discussed.**

Implementations
---------------
***Allegro#*** is the current intepreter implementation written in C#.
A C-based intepreter has been proposed but projected in further milestone.

Participate
-----------
Please contact project author [RSChiang][rschiang] for participation. We will launch our
mailing list before long.

[rschiang]: http://www.plurk.com/RSChiang
