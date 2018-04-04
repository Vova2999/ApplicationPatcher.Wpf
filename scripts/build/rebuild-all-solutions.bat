pushd %~dp0

rmdir /S /Q ..\..\Build
call build-all-solutions.bat %1

popd