

# Building Kinovea

The current build chain is not very friendly and requires old tools. Hopefully we can move to a more modern environment soon.
We've spent a good deal of time trying to make this work without the dependency on the old IDEs but there is no solution at the moment.

## Requirements
- Visual Studio 2008 + Visual Studio 2008 SP1 + VS90SP1-KB976656-x86.exe.
- Visual Studio 2010.
- Optional: a more modern IDE like VS2015 (still requires VS2008 and VS2010 installed on the machine).

## Reasons for the odd requirements

#### VS2008
- The reason for VS2008 is a combination of having a C++/CLI project, targetting .NET 3.5 and targetting x64 systems.
- Versions of Visual Studio after VS2008 don't know how to compile C++/CLI for .NET 3.5 (v90 toolset).
- Kinovea.Video.FFMpeg is C++/CLI, we would like to target .NET 3.5 for at least one more release, and also support x64 systems.
- The free VC++ Express 2008 Edition cannot be used as it doesn't support x64 build.

#### VS90SP1-KB976656-x86.exe
- This is a patch to fix a bug in VS2008SP1.

#### VS2010
- If we want to use more modern IDEs than VS2008 it's possible (VS2015 for example), but we still need to have VS2010 installed.
- Visual Studio 2008 had its own dedicated build system for C++/CLI (.vcproj files). With VS2010, C++/CLI moved to MSBuild, and we got .vcxproj files.
- Only VS2010 has the correct build scripts to handle v90 toolset.
- So even when using a recent IDE, it will delegate some operations to VS2010 to manage the transition between the build systems.

## Rebuilding

Open the solution in Visual Studio, set the build configuration to Debug and x64, rebuild the projects in the following order:
- Kinovea.Video
- Kinovea.Services
- Kinovea.Video.FFMpeg, Kinovea.Video.Bitmap, Kinovea.Video.GIF, Kinovea.Video.SVG
- Kinovea.Camera
- Kinovea.Camera.DirectShow, Kinovea.Camera.HTTP
- Kinovea.ScreenManager.
- Kinovea.FileBrowser, Kinovea.Updater
- Kinovea

The lower projects are dependent on the upper ones so you can pinpoint the problems. Usually Kinovea.Video.FFMpeg is the usual suspect. Make sure you have all the requirements as described above.




