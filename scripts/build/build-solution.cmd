@pushd %~dp0

@call nuget.exe restore %1
@call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" %1 /p:Configuration=%2 /maxcpucount:4

@popd