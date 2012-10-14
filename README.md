allegro
=======

Innovative open-source scripting language

# Grammar draft

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

    *<arg-specifier>: ( <identifier> [:<type>| (<type1>, <type2>, ...) ] [=<default-value>], ... )

Which means type-checking can be enabled by appending ":", then a type or a tuple of 
types. Interpreter will automatically throw TypeException if type mismatch during a 
call to type-check enabled function or CLR function.

    (... hold <variable-name> ...)
        <statements>
        <variable-name> = <value>

"hold" declaration can use anywhere just like variable. This statement will suspend 
current evaluation and wait until the name assigned in the *sub-scope* following it.
Interpreter will throw `CallNotFulfilledException`

    <statement> if <condition>

Syntax sugar for "if <condition>: <statement>"

    <statement> then <statement>

Line-join, just like ` _` in VB