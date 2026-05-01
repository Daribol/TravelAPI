# 🌍 Smart Travel Planner API

Smart Travel Planner is a professional **ASP.NET Core Web API** designed to help users organize their personal trips and activities. The application focuses on high performance, data security, and real-time integration with global travel data services.

## ✨ Key Features
- **Advanced Security:** Implements **JWT (JSON Web Token)** authentication and **Role-Based Access Control** (Admin vs. Regular users).
- **Comprehensive Trip Management:** Full CRUD operations for managing travel itineraries and detailed trip activities.
- **External API Integrations:** 
  - **Country Data:** Fetches capital, coordinates, and regional info via *RestCountries*.
  - **Live Weather:** Provides real-time temperature and coordinates via *Open-Meteo*.
  - **Currency Exchange:** Live budget calculations and BGN exchange rates via *Exchange Rates API*.
- **Optimized Architecture:** Fully asynchronous implementation (`async/await`) for high scalability and strict use of **DTOs** (Data Transfer Objects) to ensure data privacy and prevent circular references.

## 🛠️ Tech Stack
- **Framework:** ASP.NET Core 8.0
- **ORM:** Entity Framework Core
- **Database:** MS SQL Server
- **Authentication:** JWT Bearer Authentication
- **Documentation:** Swagger (OpenAPI)
- **Networking:** HttpClientFactory for external API orchestration

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Microsoft SQL Server (LocalDB or Express)

### Installation
1. **Clone the repository:**
   ```bash
   git clone [https://github.com/your-username/TravelAPI.git](https://github.com/your-username/TravelAPI.git)