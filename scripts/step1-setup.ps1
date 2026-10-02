# MediCareX - Step 1: solution, projects, references, NuGet packages.
# Run from the repo root in PowerShell:  .\scripts\step1-setup.ps1
# Requires: .NET 8 SDK, internet access.
$ErrorActionPreference = "Stop"
$ef = "8.0.*"

function Add-Pkgs($proj, $pkgs) {
  foreach ($p in $pkgs) {
    $name, $ver = $p -split "@"
    if ($ver) { dotnet add $proj package $name --version $ver | Out-Null }
    else      { dotnet add $proj package $name | Out-Null }
    Write-Host "  + $name $ver"
  }
}

if (-not (Test-Path MediCareX.sln)) { dotnet new sln -n MediCareX }

# Domain already exists (csproj + sources shipped in this repo). Others are created here.
if (-not (Test-Path src/MediCareX.Application))    { dotnet new classlib -n MediCareX.Application    -o src/MediCareX.Application    -f net8.0 }
if (-not (Test-Path src/MediCareX.Infrastructure)) { dotnet new classlib -n MediCareX.Infrastructure -o src/MediCareX.Infrastructure -f net8.0 }
if (-not (Test-Path src/MediCareX.API))            { dotnet new webapi   -n MediCareX.API            -o src/MediCareX.API            -f net8.0 --use-controllers }
if (-not (Test-Path src/MediCareX.Tests))          { dotnet new xunit    -n MediCareX.Tests          -o src/MediCareX.Tests          -f net8.0 }
Remove-Item src/*/Class1.cs -ErrorAction SilentlyContinue

foreach ($p in "Domain","Application","Infrastructure","API","Tests") {
  dotnet sln add "src/MediCareX.$p/MediCareX.$p.csproj" 2>$null | Out-Null
}

# References: Application->Domain, Infrastructure->Application, API->Infrastructure, Tests->all
dotnet add src/MediCareX.Application    reference src/MediCareX.Domain
dotnet add src/MediCareX.Infrastructure reference src/MediCareX.Application
dotnet add src/MediCareX.API            reference src/MediCareX.Infrastructure
dotnet add src/MediCareX.Tests          reference src/MediCareX.Domain src/MediCareX.Application src/MediCareX.Infrastructure src/MediCareX.API

Write-Host "Application packages"
Add-Pkgs src/MediCareX.Application @(
  "MediatR@12.4.1", "FluentValidation.DependencyInjectionExtensions", "AutoMapper@13.0.1",
  "Microsoft.Extensions.Logging.Abstractions@8.0.*")

Write-Host "Infrastructure packages"
Add-Pkgs src/MediCareX.Infrastructure @(
  "Microsoft.EntityFrameworkCore.SqlServer@$ef", "Microsoft.EntityFrameworkCore.Tools@$ef",
  "Microsoft.AspNetCore.Identity.EntityFrameworkCore@$ef", "Microsoft.AspNetCore.Authentication.JwtBearer@$ef",
  "Microsoft.Extensions.Caching.StackExchangeRedis@$ef", "StackExchange.Redis", "MongoDB.Driver",
  "Serilog.AspNetCore", "Hangfire.AspNetCore", "Hangfire.SqlServer", "Microsoft.Data.SqlClient",
  "Stripe.net", "Refit.HttpClientFactory", "Polly", "Microsoft.Extensions.Http.Polly@$ef",
  "Azure.Storage.Blobs", "SendGrid", "Twilio", "Nethereum.Web3", "Microsoft.ML", "Microsoft.ML.Data",
  "Otp.NET", "QRCoder", "QuestPDF", "Google.Apis.Calendar.v3", "Google.Apis.Auth",
  "Microsoft.AspNetCore.Authentication.Google@$ef", "Microsoft.AspNetCore.Authentication.Facebook@$ef",
  "Microsoft.AspNetCore.SignalR.StackExchangeRedis@$ef")
dotnet add src/MediCareX.Infrastructure package Microsoft.EntityFrameworkCore.Design --version $ef | Out-Null

Write-Host "API packages"
Add-Pkgs src/MediCareX.API @(
  "Swashbuckle.AspNetCore", "Asp.Versioning.Mvc.ApiExplorer", "AspNetCoreRateLimit", "Serilog.AspNetCore",
  "AspNetCore.HealthChecks.UI.Client", "AspNetCore.HealthChecks.SqlServer", "AspNetCore.HealthChecks.Redis",
  "AspNetCore.HealthChecks.MongoDb", "Microsoft.EntityFrameworkCore.Design@$ef")
# Note: SignalR ships in the ASP.NET Core 8 shared framework - no package needed.

Write-Host "Tests packages"
Add-Pkgs src/MediCareX.Tests @(
  "Moq", "FluentAssertions@6.12.1", "Microsoft.EntityFrameworkCore.InMemory@$ef",
  "Testcontainers.MsSql", "Microsoft.AspNetCore.Mvc.Testing@$ef", "coverlet.collector")

dotnet restore
dotnet build --nologo
Write-Host "STEP 1 + 2 complete if the build above succeeded."
