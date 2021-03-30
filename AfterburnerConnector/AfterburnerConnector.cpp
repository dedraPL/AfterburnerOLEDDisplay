#include "pch.h"
#include "AfterburnerConnector.h"
#include <string>
#include <io.h>
#include <shellapi.h>

LPVOID m_pMapAddr;
HANDLE m_hMapFile;
std::string m_strInstallPath;

void Disconnect()
{
	if (m_pMapAddr)
		UnmapViewOfFile(m_pMapAddr);

	m_pMapAddr = NULL;

	if (m_hMapFile)
		CloseHandle(m_hMapFile);

	m_hMapFile = NULL;
}

void Connect()
{
	Disconnect();
	m_hMapFile = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, "MAHMSharedMemory");

	if (m_hMapFile)
		m_pMapAddr = MapViewOfFile(m_hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, 0);
}

int ValidateConnection() 
{
	//init Afterburner installation path
	if (m_strInstallPath.empty())
	{
		HKEY hKey;
		if (ERROR_SUCCESS == RegOpenKey(HKEY_LOCAL_MACHINE, "Software\\MSI\\Afterburner", &hKey))
		{
			char buf[MAX_PATH];
			DWORD dwSize = MAX_PATH;
			DWORD dwType;

			if (ERROR_SUCCESS == RegQueryValueEx(hKey, "InstallPath", 0, &dwType, (LPBYTE)buf, &dwSize))
			{
				if (dwType == REG_SZ)
					m_strInstallPath = buf;
			}

			RegCloseKey(hKey);
		}
	}

	//validate Afterburner installation path
	//_access with second argument = 0 checks if file exist
	if (_access(m_strInstallPath.c_str(), 0))
	{
		m_strInstallPath = "";
	}

	//connect to Afterburner shared memory if not connected yet
	if (!m_pMapAddr)
		Connect();

	//check if connection to shared memory is valid
	if (m_pMapAddr)
	{
		LPMAHM_SHARED_MEMORY_HEADER lpHeader = (LPMAHM_SHARED_MEMORY_HEADER)m_pMapAddr;

		//if connection is not valid, reconnect
		if (lpHeader->dwSignature == 0xDEAD)
			Connect();
	}

	if (m_pMapAddr)
	{
		LPMAHM_SHARED_MEMORY_HEADER	lpHeader = (LPMAHM_SHARED_MEMORY_HEADER)m_pMapAddr;

		//check if connected to valid memory
		if (lpHeader->dwSignature == 'MAHM')
		{
			if (lpHeader->dwVersion >= 0x00020000)
			{
				return 1;
			}
			else
			{
				return ERROR_AFTERBURNER_VERSION_OLD;
			}
		}
		else
		{
			return ERROR_AFTERBURNER_NOT_INITIALIZED;
		}
	}
	else
	{
		if (m_strInstallPath.empty())
			return ERROR_AFTERBURNER_NOT_INSTALLED;
		else
			return ERROR_AFTERBURNER_NOT_STARTED;
	}
}

std::string dumpGpuEntry(LPMAHM_SHARED_MEMORY_GPU_ENTRY lpGpuEntry)
{
	std::string deviceName;
	if (strlen(lpGpuEntry->szDevice))
	{
		deviceName = lpGpuEntry->szDevice;
	}
	deviceName += ',';
	if (strlen(lpGpuEntry->szGpuId))
	{
		deviceName += lpGpuEntry->szGpuId;
	}
	return deviceName;
}

bool FilterSystemSources(DWORD id)
{
	switch (id)
	{
	case MONITORING_SOURCE_ID_CPU_TEMPERATURE:
	case MONITORING_SOURCE_ID_CPU_USAGE:
	case MONITORING_SOURCE_ID_RAM_USAGE:
	case MONITORING_SOURCE_ID_PAGEFILE_USAGE:
	case MONITORING_SOURCE_ID_CPU_CLOCK:
	case MONITORING_SOURCE_ID_FRAMERATE:
	case MONITORING_SOURCE_ID_FRAMETIME:
		return true;
	}
	return false;
}

std::string dumpSystemEntry(LPMAHM_SHARED_MEMORY_ENTRY lpEntry)
{
	char tmp[10] = { 0 };
	
	if (lpEntry->data != FLT_MAX)
	{
		snprintf(tmp, 10, lpEntry->szRecommendedFormat, lpEntry->data);
	}
	std::string data(tmp);
	return data;
}

extern "C" __declspec(dllexport) const void __cdecl lunchAfterburner() 
{
	int res = ValidateConnection();
	if (!m_strInstallPath.empty() && res == ERROR_AFTERBURNER_NOT_STARTED)
	{
		ShellExecute(NULL, "open", m_strInstallPath.c_str(), m_pMapAddr ? "-propertypage2" : "-m", NULL, SW_SHOWNORMAL);
		//if MSI Afterburner is not running yet then start and force it to be minimized to system tray with -m command line switch,
		//otherwise just start it to force running instance it to be open from system tray
	}
}

extern "C" __declspec(dllexport) const int __cdecl obtainGPUData(char* buf, unsigned int len)
{
	int res = ValidateConnection();

	if (res > 0) {
		memset(buf, 0, len);
		std::string data;
		data.reserve(len);

		LPMAHM_SHARED_MEMORY_HEADER	lpHeader = (LPMAHM_SHARED_MEMORY_HEADER)m_pMapAddr;

		DWORD dwGpus = lpHeader->dwNumGpuEntries;
		
		for (DWORD dwGpu = 0; dwGpu < dwGpus; dwGpu++)
		{
			LPMAHM_SHARED_MEMORY_GPU_ENTRY lpGpuEntry = (LPMAHM_SHARED_MEMORY_GPU_ENTRY)((LPBYTE)lpHeader + lpHeader->dwHeaderSize + lpHeader->dwNumEntries * lpHeader->dwEntrySize + dwGpu * lpHeader->dwGpuEntrySize);

			data += std::to_string(dwGpu);
			data += '=';
			data += dumpGpuEntry(lpGpuEntry);
			if (dwGpu + 1 < dwGpus)
				data += ';';
		}
		memcpy(buf, data.c_str(), data.length() < len ? data.length() : len);
		return data.length() < len ? data.length() : len;
	}
	else
	{
		return res;
	}
}

extern "C" __declspec(dllexport) const int __cdecl obtainData(char* buf, size_t len, DWORD dwGpu = 0)
{
	int res = ValidateConnection();

	if (res > 0) {
		memset(buf, 0, len);
		std::string data;
		data.reserve(len);

		LPMAHM_SHARED_MEMORY_HEADER	lpHeader = (LPMAHM_SHARED_MEMORY_HEADER)m_pMapAddr;

		if (dwGpu >= lpHeader->dwNumGpuEntries)
			return ERROR_WRONG_GPU_ID;

		//obtain gpu name
		LPMAHM_SHARED_MEMORY_GPU_ENTRY lpGpuEntry = (LPMAHM_SHARED_MEMORY_GPU_ENTRY)((LPBYTE)lpHeader + lpHeader->dwHeaderSize + lpHeader->dwNumEntries * lpHeader->dwEntrySize + dwGpu * lpHeader->dwGpuEntrySize);
		data += dumpGpuEntry(lpGpuEntry);

		//obtain gpu temperature and usage
		for (DWORD dwSource = 0; dwSource < lpHeader->dwNumEntries; dwSource++)
		{
			LPMAHM_SHARED_MEMORY_ENTRY lpEntry = (LPMAHM_SHARED_MEMORY_ENTRY)((LPBYTE)lpHeader + lpHeader->dwHeaderSize + dwSource * lpHeader->dwEntrySize);

			if (FilterSystemSources(lpEntry->dwSrcId))
				continue;

			if (lpEntry->dwGpu != dwGpu)
				continue;

			data += ';';

			if (lpEntry->dwSrcId == MONITORING_SOURCE_ID_GPU_TEMPERATURE)
				data += GPU_TEMPERATURE_PREFIX;
			else if (lpEntry->dwSrcId == MONITORING_SOURCE_ID_GPU_USAGE)
				data += GPU_USAGE_PREFIX;
			else
				data += OTHER_PREFIX;

			std::string tmp = dumpSystemEntry(lpEntry);
			data += "=" + (tmp.length() > 0 ? tmp : "0");
		}

		//obtain system data
		for (DWORD dwSource = 0; dwSource < lpHeader->dwNumEntries; dwSource++)
		{
			LPMAHM_SHARED_MEMORY_ENTRY lpEntry = (LPMAHM_SHARED_MEMORY_ENTRY)((LPBYTE)lpHeader + lpHeader->dwHeaderSize + dwSource * lpHeader->dwEntrySize);

			if (!FilterSystemSources(lpEntry->dwSrcId))
				continue;

			data += ';';

			if (lpEntry->dwSrcId == MONITORING_SOURCE_ID_CPU_TEMPERATURE)
				data += CPU_TEMPERATURE_PREFIX;
			else if (lpEntry->dwSrcId == MONITORING_SOURCE_ID_CPU_USAGE)
				data += CPU_USAGE_PREFIX;
			else if (lpEntry->dwSrcId == MONITORING_SOURCE_ID_FRAMERATE)
				data += FRAMERATE_PREFIX;
			else
				data += OTHER_PREFIX;

			std::string tmp = dumpSystemEntry(lpEntry);
			data += "=" + (tmp.length() > 0 ? tmp : "0");
		}

		memcpy(buf, data.c_str(), data.length() < len ? data.length() : len);
		return data.length() < len ? data.length() : len;
	}
	else
	{
		return res;
	}
}