@echo off
pushd %~dp0
call scripts\run-project.bat ApplicationPatcher.Wpf.sln
popd
@echo on