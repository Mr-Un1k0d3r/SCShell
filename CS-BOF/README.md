# BOF Version of SCShell for Cobalt Strike

## How to compile 

Use the provided [Makefile](./Makefile) (currently implemented
for the Linux MinGW-w64 cross-compiler):

```
make
```

## Using the BOF in Cobalt Strike

Simply load the aggressor script [scshell.cna](./scshell.cna).

Then you have the beacon command `scshell`. For getting cmd options:

```
help scshell
```

Two authentication methods are implemented (in different BOFs):

* impersonating the current process token
* using the logon session linked to the process token (useful with `make_token` or `pth`)

## Lateral movement using builtin jump

Also implemented the lateral movement technique using the internal
`jump` beacon command, for listing available methods
(`scshell` and `scshell64` should be there):

```
jump
```

For using `jump`:

```
help jump
```

There are configurable setting for the `jump scshell` and the
`jump scshell64` technqiues. For querying and setting the options, use:

```
scshell-settings
```

For help:

```
help scshell-settings
```

Note that there is an optional configurable delay between uploading
the artifact on the remote target and executing it, this might be
useful for specific EDR evasion.
