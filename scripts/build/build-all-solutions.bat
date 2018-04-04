@pushd %~dp0

@tasklist | find "MSBuild.exe" > nul
@if %errorLevel%==0 taskkill /IM MSBuild.exe /f

@call build-solution.bat ..\..\_source\ApplicationPatcher.Wpf.sln %1

@tasklist | find "MSBuild.exe" > nul
@if %errorLevel%==0 taskkill /IM MSBuild.exe /f

@if %1==Release del /S /Q ..\..\Build\*.pdb > nul

@popd