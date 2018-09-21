
PylonC.NET.dll is a thin wrapper around PylonC_MD_VC100.dll and is part of the SDK published by Basler as part of their Software suite. 
During Basler software suite installation, the options to install the Pylon C runtime and add the environment variable PYLONC_ROOT must be checked.

Copying to output
The .NET framework will only delay-load the assembly as we don't need it in any public field.
This means the Pylon wrapper DLL will not be copied to the output directory.
Even if we use it here, since the Pylon installer added it to the GAC, it won't be copied to the output.
The copy is done explicitely in kinovea.targets.
