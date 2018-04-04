@echo off
pushd %~dp0
call scripts\build\rebuild-all-solutions.bat Debug
popd
@echo on