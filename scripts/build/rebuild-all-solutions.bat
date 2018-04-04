@pushd %~dp0

@rmdir /S /Q ..\..\Build > nul 2>&1
@call build-all-solutions.bat %1

@popd