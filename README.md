# 🌐 .NET Core Web API – CRUD Microservice

This repository contains a modular .NET Core Web API implementing full CRUD operations. The project follows a layered architecture with clear separation of concerns across controller, business, and data access layers.

---

## 🏗️ Architecture Overview

- **Controllers/**  
  Defines RESTful endpoints for CRUD operations.

- **BusinessLayer/**  
  Contains service logic and interfaces for business rules.

- **DataAccessLayer/**  
  Implements repository pattern for database interaction.

- **Model/**  
  Domain models representing entities used across layers.

---

## 🛠️ Getting Started

### Prerequisites

- [.NET SDK 6+](https://dotnet.microsoft.com/download)
- SQL Server (or your preferred RDBMS)

### Installation

```bash
git clone https://github.com/pritpalkaur/dotnetcoreWebAPI205.git
cd dotnetcoreWebAPI205/API
