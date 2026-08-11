# Labor Management System

A comprehensive labor management system built with ASP.NET Core Web API, implementing clean architecture patterns with JWT authentication, Dapper ORM, and SQL Server.

## 🏗️ System Architecture

The system follows clean architecture principles with the following project structure:

- **Labor.API** - Web API controllers and configuration
- **Labor.Models** - Entities, DTOs, and data models
- **Labor.DataAccess** - Data access layer using Dapper
- **Labor.Auth** - JWT authentication and authorization services

## 🚀 Key Features

### User Management
- **Three User Roles**: Admin, Labor, Employer
- **Mobile-based Authentication**: Mobile number as username
- **OTP Verification**: Mandatory for registration and password reset
- **JWT Token Authentication**: Secure API access
- **Role-based Permissions**: Dynamic feature access control

### Labor Management
- **Labor Types**: Normal, Plumber, Electrician, Carpenter, etc.
- **Location-based Search**: Find nearby labor using GPS coordinates
- **Skills & Ratings**: Labor skill tracking and rating system
- **Availability Status**: Real-time availability management

### E-commerce Features
- **Add to Cart**: Labor booking with hours and descriptions
- **Order Management**: Complete order lifecycle tracking
- **Order History**: Historical order tracking and status updates
- **Reviews & Ratings**: Labor review and rating system

### Admin Features
- **Labor Onboarding**: Admin-only labor user creation
- **Role Management**: Dynamic permission assignment
- **System Configuration**: Labor types and system settings
- **Complete Access**: Full system administration capabilities

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

## 🛠️ Setup Instructions

### 1. Database Setup

1. Create the database and schema:
```sql
-- Run the scripts in order:
sqlcmd -S localhost -i "Database/01_CreateDatabase.sql"
sqlcmd -S localhost -i "Database/02_StoredProcedures.sql"
sqlcmd -S localhost -i "Database/03_Functions.sql"
sqlcmd -S localhost -i "Database/04_InitialData.sql"
```

2. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=LaborManagementDB;Trusted_Connection=true;"
  }
}
```

### 2. Application Setup

1. Restore NuGet packages:
```bash
dotnet restore
```

2. Update JWT configuration in `appsettings.json`:
```json
{
  "JWT": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "LaborManagementSystem",
    "Audience": "LaborManagementUsers",
    "ExpiryHours": 24
  }
}
```

3. Run the application:
```bash
dotnet run --project Labor.API
```

## 📚 API Endpoints

### Authentication Endpoints

```http
POST /api/auth/request-otp
POST /api/auth/register
POST /api/auth/login
POST /api/auth/change-password
```

### Labor Search Endpoints

```http
GET /api/labor/search
GET /api/labor/{id}
GET /api/labor/types
```

### Cart Management Endpoints

```http
GET /api/cart
POST /api/cart/add
PUT /api/cart/{id}
DELETE /api/cart/{id}
```

### Order Management Endpoints

```http
GET /api/orders
POST /api/orders
GET /api/orders/{id}
PUT /api/orders/{id}/status
```

## 🔐 Authentication Flow

### Employer Registration
1. Request OTP: `POST /api/auth/request-otp`
2. Register with OTP: `POST /api/auth/register`
3. Receive JWT token for API access

### Labor Onboarding (Admin Only)
1. Admin creates labor user account
2. Labor receives temporary password
3. Labor must change password on first login

### Login Process
1. Login with mobile/password: `POST /api/auth/login`
2. Receive JWT token
3. Use token in Authorization header: `Bearer {token}`

## 🗄️ Database Schema

### Key Tables
- `[User].[Users]` - User accounts and profiles
- `[User].[Roles]` - System roles (Admin, Labor, Employer)
- `[User].[Addresses]` - User address management
- `[Labor].[Labors]` - Labor profiles and details
- `[Labor].[LaborTypes]` - Labor categories and rates
- `[Order].[Orders]` - Order management
- `[Order].[Carts]` - Shopping cart functionality
- `[System].[OTPVerifications]` - OTP management

## 🔧 Configuration

### SMS Integration
Update `OTPService.cs` to integrate with your SMS provider:

```csharp
public async Task<bool> SendOTPAsync(string mobileNumber, string otp, string purpose)
{
    // Integrate with SMS gateway (Twilio, AWS SNS, etc.)
    // Current implementation logs OTP for development
}
```

### Email Configuration
Add email service for notifications and password recovery.

## 🚦 Default Credentials

After running the initial data script:

**Admin Account:**
- Mobile: 1234567890
- Password: admin123

**Note:** Change default password immediately after first login.

## 🔍 Labor Types

Default labor types available in the system:
- Normal (General labor) - ₹50/hour
- Plumber - ₹150/hour
- Electrician - ₹200/hour
- Carpenter - ₹120/hour
- Painter - ₹80/hour
- Cleaner - ₹60/hour
- Gardener - ₹70/hour

## 📱 Mobile App Integration

The API is designed to work with mobile applications:
- Location-based labor search using GPS
- Real-time order tracking
- Push notifications for order updates
- Image upload support for profiles and work completion

## 🔒 Security Features

- JWT token-based authentication
- Password hashing with BCrypt
- OTP verification for secure registration
- Role-based access control
- SQL injection protection with parameterized queries
- CORS configuration for web client support

## 🚀 Deployment

### Production Configuration

1. Update connection string for production database
2. Configure secure JWT secret key
3. Set up SMS gateway for OTP delivery
4. Configure HTTPS certificates
5. Set up logging and monitoring

### Environment Variables

```bash
JWT_SECRET_KEY=your_production_secret_key
DB_CONNECTION_STRING=your_production_db_connection
SMS_API_KEY=your_sms_provider_key
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to the branch
5. Create Pull Request

## 📄 License

This project is licensed under the MIT License.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Email: support@labormanagement.com

## 🔄 Version History

- **v1.0.0** - Initial release with core functionality
- Authentication system
- Labor search and booking
- Order management
- Admin dashboard features

---

**Note:** This system is designed for production use but requires proper SMS gateway integration and additional security configurations for live deployment. 