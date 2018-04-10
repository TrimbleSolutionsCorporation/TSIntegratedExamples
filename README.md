# TSIntegratedExamples

Tekla Structures API Examples with integrated build system and installers
1. AnchorBoltsSimple: Console application to add dimensions to plan views on active drawing for anchor bolts, no user interface, free placement.
2. AnchorBoltsWinform: Winform application to add dimensions to plan views on active drawing for anchor bolts. Has user interface allowing for settings.
      Calculates offset from pier solid and places dimensions outside pier with offset distance, fixed placement.
*See \Test Models\Example AnchorBolts.zip for model to test with source code.

Setup:
>Direct output for all projects is re-directed to \Output\bin or \Output\bin_release
>Intermediate output for all projects re-directed \Intermediate\
>Separate target action copies direct output to each project Installer sub-folder
>Each project includes Manifest.xml to allow manual creation of TSEP

+Select a different branch to see examples for that version of Tekla Structures