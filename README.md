# IQA Management Hub

[![CI pipeline](https://github.com/iqasport/referee_hub/actions/workflows/pipeline.yml/badge.svg)](https://github.com/iqasport/referee_hub/actions/workflows/pipeline.yml)
[![codecov](https://codecov.io/gh/iqasport/referee_hub/branch/master/graph/badge.svg)](https://codecov.io/gh/iqasport/referee_hub)
[![CodeFactor](https://www.codefactor.io/repository/github/iqasport/referee_hub/badge)](https://www.codefactor.io/repository/github/iqasport/referee_hub)

## Overview

The **IQA Management Hub** is a comprehensive platform designed to support the [International Quadball Association (IQA)](https://iqasport.org/) in managing referee certifications, testing, and operations across its global network of National Governing Bodies (NGBs).

### What is the International Quadball Association?

The International Quadball Association (IQA) is the international governing body for the sport of quadball (formerly known as quidditch). The IQA oversees the sport's development worldwide, coordinates international competitions, and establishes rules and standards for play.

### What the Hub Offers

The Management Hub provides essential tools for:

**For Referees:**
- **Certification Management**: Track and manage referee certifications at various levels
- **Online Testing**: Complete certification tests with automatic grading and result tracking
- **Profile Management**: Maintain referee profiles with certification history and achievements
- **Test History**: View past test attempts, scores, and certification progress
- **Payment Processing**: Secure payment processing for certification fees via Stripe integration

**For National Governing Bodies (NGBs):**
- **Referee Oversight**: View and manage referees within their jurisdiction
- **Data Export**: Export referee and team data for reporting and analysis
- **NGB Profile Management**: Maintain organization profiles with branding and media
- **Statistics and Reporting**: Access referee certification statistics and trends

**For Administrators:**
- **User Management**: Manage user accounts, roles, and permissions
- **Test Administration**: Create and manage certification tests and questions in multiple languages
- **Test Content**: Create, edit, and organize test questions and certifications
- **Import Tools**: Bulk import data using CSV import wizards
- **System Configuration**: Configure system settings, languages, and test policies

## Project Structure

The IQA Management Hub is a modern web application built with a clear separation between frontend and backend.

### Frontend (`src/frontend/`)

**Technology Stack:**
- **React 18** - Modern component-based UI framework
- **TypeScript** - Type-safe JavaScript for better developer experience
- **Redux Toolkit** - State management with @reduxjs/toolkit
- **React Router 6** - Client-side routing
- **Tailwind CSS** - Utility-first CSS framework
- **PostCSS** - CSS processing and optimization
- **Webpack 5** - Module bundling and build tooling
- **Jest** - Unit testing framework
- **Axios** - HTTP client for API communication
- **Stripe** - Payment processing integration

**Module Structure:**
- `app/modules/` - Feature modules organized by domain
- `app/pages/` - Top-level page components
- `app/components/` - Reusable UI components
- `app/apis/` - API client code (generated from OpenAPI specs)
- `app/store/` - Redux store configuration

### Backend (`src/backend/`)

**Technology Stack:**
- **.NET 8.0** - Modern, cross-platform application framework
- **C#** - Primary programming language
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - Object-relational mapping (ORM)
- **PostgreSQL** - Relational database (production)
- **Redis** - Caching and background job queue
- **Hangfire** - Background job processing
- **Stripe.NET** - Payment processing
- **Swashbuckle** - OpenAPI/Swagger documentation
- **xUnit** - Unit testing framework
- **OpenTelemetry** - Distributed tracing and monitoring

**Project Structure:**
- `ManagementHub.Service/` - Main web API service
  - `Areas/` - Feature areas organized by domain:
    - `Debug/` - Debugging and development endpoints
    - `Export/` - Data export functionality
    - `Identity/` - Authentication and authorization
    - `Languages/` - Language management
    - `Ngbs/` - National Governing Body endpoints
    - `Payments/` - Payment processing
    - `Referees/` - Referee management
    - `Tests/` - Test and certification endpoints
    - `User/` - User management
  - `Authorization/` - Authorization policies and handlers
  - `Configuration/` - Application configuration
  - `Jobs/` - Background job definitions
  - `Swagger/` - API documentation configuration
  - `Telemetry/` - Observability and monitoring
- `ManagementHub.Models/` - Domain models and data structures
- `ManagementHub.Storage/` - Data access layer and database context
- `ManagementHub.Processing/` - Business logic and domain services
- `ManagementHub.Serialization/` - JSON serialization configuration
- `ManagementHub.Mailers/` - Email notification services
- `ManagementHub.UnitTests/` - Unit test project

### Infrastructure

**Docker:**
- Multi-stage Docker builds for containerized deployment
- Docker Compose configurations for local development with dependencies
- Configurations available in `docker/` directory for dev, staging, and production environments

**CI/CD:**
- GitHub Actions pipeline (`.github/workflows/pipeline.yml`)
- Automated building, testing, linting, and security scanning
- CodeQL security analysis
- Docker image publishing to AWS ECR
- Deployment to AWS Elastic Beanstalk (production)

**Observability:**
- OpenTelemetry instrumentation for distributed tracing
- Support for Azure Monitor and OTLP exporters
- Structured logging with request correlation

## Building Instructions

### Prerequisites

- **.NET SDK 8.0** or later ([download](https://dotnet.microsoft.com/download))
- **Node.js 18.x** or later ([download](https://nodejs.org/))
- **Yarn** package manager ([install](https://yarnpkg.com/getting-started/install))
- **PostgreSQL** database (optional - only needed if not using in-memory database)
- **Redis** (optional - only needed if not using in-memory cache and job queue)

### Quick Start with Docker

For the fastest development setup, use Docker Compose:

```bash
# Navigate to the docker dev directory
cd docker/dev

# Start all services (frontend, backend, database, redis)
docker compose up -d
```

The application will be available at `http://localhost:80`.

### Local Development Setup

#### 1. Clone the Repository

```bash
git clone https://github.com/iqasport/referee_hub.git
cd referee_hub
```

#### 2. Build the Frontend

```bash
cd src/frontend

# Install dependencies
yarn install --immutable

# Build for development
yarn build:dev

# Or build for production
yarn build:prod
```

The frontend build includes:
- JavaScript bundling with Webpack
- CSS compilation with PostCSS and Tailwind
- Image asset copying

#### 3. Build the Backend

```bash
cd src/backend

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

The backend build automatically includes the frontend assets if they're available in `src/frontend/dist/`.

#### 4. Run the Application

**Development Mode (In-Memory Dependencies):**

```bash
cd src/backend/ManagementHub.Service

# Run with hot reload
dotnet run
```

The service will start on `http://localhost:5000` with in-memory database and cache.

**With Docker Compose (Real Dependencies):**

For a more production-like environment with PostgreSQL, Redis, and MailHog:

```bash
cd docker/staging
docker compose up -d
```

The service will be available at `http://localhost:80`.

## Running Instructions

### Running the Backend Service

The backend service can be run in different modes depending on your development needs:

**Quick Start (In-Memory Mode):**

The simplest way to run the service is with in-memory dependencies:

```bash
cd src/backend/ManagementHub.Service
dotnet run
```

The service will start at `http://localhost:5000` with:
- In-memory database (auto-seeded with test data)
- In-memory cache
- In-memory job queue
- Debug email output (emails logged to console)
- Local filesystem blob storage

**Test Users:**

When running in development mode with `SeedDatabaseWithTestData: true`, the following test users are available (all passwords are `password`):

- **Referee**: `referee@example.com`
- **NGB Admin**: `ngb_admin@example.com`
- **IQA Admin**: `iqa_admin@example.com`
- **Empty Name Referee**: `empty@example.com`

**Configuration Options:**

You can control which dependencies are real vs. in-memory by editing `appsettings.Development.json` in the `Services` section:

```json
{
  "Services": {
    "UseInMemoryDatabase": true,      // Set to false to use PostgreSQL
    "SeedDatabaseWithTestData": true, // Auto-seed test data on startup
    "UseInMemoryJobSystem": true,     // Set to false to use Hangfire with Redis
    "UseLocalFilesystemBlobStorage": true,
    "UseDebugMailer": true           // Set to false for real email sending
  }
}
```

**Running with Real Dependencies:**

To run with PostgreSQL and Redis, you can use Docker Compose:

```bash
cd docker/staging
docker compose up -d
```

This starts:
- The Management Hub service
- PostgreSQL database
- Redis cache and job queue
- MailHog (email testing tool at `http://localhost:8025`)

**Hot Reload for Frontend Changes:**

While the backend service is running, you can rebuild the frontend without restarting:

```bash
# In a separate terminal
cd src/frontend
yarn build:dev
```

The backend will automatically pick up the updated frontend files on the next page refresh. Note that this works best when running `dotnet run` directly rather than through Docker.

**Docker Configurations:**

Several Docker Compose configurations are available in the `docker/` directory:
- `docker/dev/` - Development setup with latest Docker image
- `docker/dev-https/` - Development with HTTPS
- `docker/staging/` - Full local stack with all dependencies
- `docker/staging-https/` - Staging with HTTPS
- `docker/prod-https/` - Production-like setup with HTTPS

### Running Tests

**Frontend Tests:**

```bash
cd src/frontend

# Run tests with coverage
yarn test
```

**Backend Tests:**

```bash
cd src/backend

# Run all unit tests
dotnet test
```

### Linting

**Frontend:**

```bash
cd src/frontend

# Run ESLint
yarn lint
```

### Building Docker Image

To build a production Docker image:

```bash
cd src/backend

# Publish and build Docker image
dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer
```

The image will be tagged as `iqasport/management-hub:latest`.

### API Documentation

When running the application in Development mode, Swagger UI is available at:
- `http://localhost:5000/swagger`

The API documentation is automatically generated from the C# code using Swashbuckle.

### Additional Documentation

- **Building Details**: See [docs/building.md](docs/building.md) for detailed build instructions
- **Testing Guide**: See [docs/testing.md](docs/testing.md) for testing information
- **API Client**: See [docs/api-client.md](docs/api-client.md) for API client generation
- **AWS Deployment**: See [docs/aws.md](docs/aws.md) for AWS-specific deployment details


