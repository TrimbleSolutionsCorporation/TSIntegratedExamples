# TSIntegratedExamples

*Note: This is a more advanced example set with basic msbuild configuration setup for larger code base managment...

Tekla Structures API Examples with integrated build system and installers
Repo Goal:
1. Show integrated TSEP installer, msbuild setup, nuget packages managed centrally at solution level
2. Example TSEP type installers using wildcards and folder structure
3. Specific examples requested by various customers

Contents:
1. AnchorBoltsSimple: Console application to add dimensions to plan views on active drawing for anchor bolts, no user interface, free placement.
2. AnchorBoltsWinform: Winform application to add dimensions to plan views on active drawing for anchor bolts. Has user interface allowing for settings.
      Calculates offset from pier solid and places dimensions outside pier with offset distance, fixed placement.
*See \Test Models\Example AnchorBolts.zip for model to test with source code.

Setup:
>Projects use NuGet references
>Direct output for all projects is re-directed to new output dir
>Intermediate output for all projects re-directed \Intermediate\
>Separate target action copies direct output to each project Installer sub-folder
>Each project includes Manifest.xml to allow manual creation of TSEP
>TSEP can be created from Manifest.xml using TeklaExtensionPackage.Builder.exe included in Tekla Structures main installer ..\Program Files\<version>\nt\bin\

Build Configuration:
>All projects automatically read Directory.Build.props and Directory.Build.targets
>Directory.Build.props sets variables, platform, configuration, default settings, custom properties and targets
>Each project has PropertyGroup section in .csproj for setting custom variables for target cleaning and copying of output as well as to set the sub-folder name using installer output
>Reference package versions are set in Packages.props at solution level

+Select a different branch to see examples for that version of Tekla Structures
