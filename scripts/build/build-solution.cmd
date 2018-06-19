@pushd %~dp0

@call nuget.exe restore %1
@call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\MSBuild.exe" %1 /p:Configuration=%2 /maxcpucount:4

@popd