@pushd %~dp0
@call ..\ApplicationPatcher\.rebuild-release.bat
@call ..\ApplicationPatcher\Build\ApplicationPatcher.Self\ApplicationPatcher.Self.exe
@popd