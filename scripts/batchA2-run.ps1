# Run from repo root AFTER scripts\batchA-setup.ps1 succeeded.
$ErrorActionPreference = "Stop"

# Swashbuckle 6.x keeps the Microsoft.OpenApi.Models API used in Program.cs
dotnet add src/MediCareX.API package Swashbuckle.AspNetCore --version 6.9.0

# Remove the template sample
Remove-Item src/MediCareX.API/WeatherForecast.cs -ErrorAction SilentlyContinue
Remove-Item src/MediCareX.API/Controllers/WeatherForecastController.cs -ErrorAction SilentlyContinue
Remove-Item src/MediCareX.API/*.http -ErrorAction SilentlyContinue

dotnet build MediCareX.sln --nologo
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED - paste the errors to Claude" -ForegroundColor Red; exit 1 }

dotnet tool update --global dotnet-ef --version "8.*"
dotnet ef migrations add InitialCreate -p src/MediCareX.Infrastructure -s src/MediCareX.API -o Persistence/Migrations
dotnet ef database update -p src/MediCareX.Infrastructure -s src/MediCareX.API

Write-Host "`nDone. Start the API with:  dotnet run --project src/MediCareX.API   then open /swagger" -ForegroundColor Green
