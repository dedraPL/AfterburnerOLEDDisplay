#pragma once

#include "MAHMSharedMemory.h"

//errro codes
#define ERROR_AFTERBURNER_NOT_STARTED -1
#define ERROR_AFTERBURNER_NOT_INSTALLED -2
#define ERROR_AFTERBURNER_NOT_INITIALIZED -3
#define ERROR_AFTERBURNER_VERSION_OLD -4

#define ERROR_WRONG_GPU_ID -5

#define GPU_TEMPERATURE_PREFIX "GT"
#define GPU_USAGE_PREFIX "GU"
#define CPU_TEMPERATURE_PREFIX "CT"
#define CPU_USAGE_PREFIX "CU"
#define FRAMERATE_PREFIX "FR"
#define OTHER_PREFIX "UU"