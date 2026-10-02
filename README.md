
@'
# 🏥 MediCareX - AI-Powered Smart Healthcare System

An intelligent healthcare platform built with **ASP.NET Core 8** featuring AI-powered disease prediction, video consultation, and comprehensive healthcare management.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-purple)](https://dotnet.microsoft.com/apps/aspnet)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Build](https://img.shields.io/badge/Build-Passing-brightgreen)]()

## 📋 Overview

MediCareX is a comprehensive healthcare platform designed for Final Year Project (FYP). It provides:

- 🤖 **AI-Powered Disease Prediction** (ML.NET + Python FastAPI)
- 📹 **Real-Time Video Consultation** (WebRTC + SignalR)
- 💬 **Live Chat** between patients and doctors
- 📝 **E-Prescription** with QR verification
- 💳 **Multi-Gateway Payments** (Stripe, JazzCash, EasyPaisa, PayPal)
- 📊 **Admin Analytics Dashboard**
- 🔐 **JWT + 2FA Authentication**
- 🏥 **Complete Healthcare Management**

## 🏗️ Architecture

- **Clean Architecture** (Domain, Application, Infrastructure, API)
- **CQRS** with MediatR
- **Repository Pattern + Unit of Work**
- **Domain-Driven Design** principles

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8 Web API |
| Database | SQL Server 2022, Redis, MongoDB |
| ORM | Entity Framework Core 8 |
| Auth | JWT + Refresh Tokens + Identity + 2FA |
| Real-time | SignalR, WebRTC |
| AI/ML | ML.NET, Python FastAPI |
| Payments | Stripe, JazzCash, EasyPaisa, PayPal |
| Logging | Serilog |
| Validation | FluentValidation |
| Mapping | Mapperly |
| Background Jobs | Hangfire |
| Deployment | Docker, GitHub Actions |

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server 2022 (or LocalDB)
- Redis (optional for dev)
- MongoDB (optional for dev)
- Visual Studio 2022 / VS Code

### Setup

```bash
# Clone repository
git clone https://github.com/YOUR_USERNAME/MediCareX.git
cd MediCareX

# Restore packages
dotnet restore

# Apply migrations
dotnet ef database update -p src/MediCareX.Infrastructure -s src/MediCareX.API

# Run API
dotnet run --project src/MediCareX.API

# Swagger UI
# https://localhost:7102/swagger