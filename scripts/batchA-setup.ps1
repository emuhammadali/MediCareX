# Batch A prerequisites. Run once from repo root BEFORE building the new Application code.
$ErrorActionPreference = "Stop"
dotnet remove src/MediCareX.Application package AutoMapper           # vulnerable (NU1903) / now commercial
dotnet add    src/MediCareX.Application package Riok.Mapperly       # free compile-time mapper (used from Batch B)
dotnet add    src/MediCareX.Application package Microsoft.EntityFrameworkCore --version 8.0.*
dotnet build MediCareX.sln --nologo
