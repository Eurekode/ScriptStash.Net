REM  ...create NuGet package...
CD "..\"
nuget pack -Properties Configuration=Release -OutputDirectory ".\package"
pause