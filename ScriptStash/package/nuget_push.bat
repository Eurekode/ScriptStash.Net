REM  ...Push nuget package...
CD "..\bin\Release"
dotnet nuget push ScriptStash.Net.2.0.0.nupkg --api-key ########### --source https://api.nuget.org/v3/index.json
pause