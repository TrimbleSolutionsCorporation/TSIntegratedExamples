# TSIntegratedExamples

Tekla Structures API Examples with integrated build system and installers
Repo Goal:
1. Show integrated TSEP installer, msbuild setup, nuget packages all integrated together
2. Example TSEP type installers using wildcards and folder structure
3. Specific examples requested by various customers

Contents:
1. AnchorBoltsSimple: Console application to add dimensions to plan views on active drawing for anchor bolts, no user interface, free placement.
2. AnchorBoltsWinform: Winform application to add dimensions to plan views on active drawing for anchor bolts. Has user interface allowing for settings.
      Calculates offset from pier solid and places dimensions outside pier with offset distance, fixed placement.
*See \Test Models\Example AnchorBolts.zip for model to test with source code.

Setup:
>Projects use NuGet references
>Direct output for all projects is re-directed to \Output\bin or \Output\bin_release
>Intermediate output for all projects re-directed \Intermediate\
>Separate target action copies direct output to each project Installer sub-folder
>Each project includes Manifest.xml to allow manual creation of TSEP
>TSEP can be created from Manifest.xml using TeklaExtensionPackage.Builder.exe included in Tekla Structures main installer ..\Program Files\<version>\nt\bin\

Build Configuration:
>All projects import root_buildsetup.props and root_configuration.props
>root_configuration.props is imported first to set variables for 3rd party tools and Tekla version and package numbers as well as some custom variables fro build actions
>TeklaOpenAPI.props from NuGet references is then imported, this was added as part of NuGet reference when added through NuGet Package Manager
>root_buildsetup.props is imported last since it overrides some settings in TeklaOpenAPI.props like BINDir, OBJDir, and IntermediateOutputPath
>Each project has PropertyGroup section for setting custom variables for target cleaning and copying of output as well as to set the sub-folder name using installer output
>References for Tekla Structures have manually edited so Version=$(TSVersionNumber) and $(TeklaPackageVerNo are dynmic and version independent
>Because the direct output build often adds additional references not required, there is target clean action on copied binary output in Installer sub-folder

+Select a different branch to see examples for that version of Tekla Structures
