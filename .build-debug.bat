@echo off
pushd %~dp0
call scripts\build\build-all-solutions.bat Debug
popd
@echo on