# Labor Management System API Documentation

Base URL: `https://localhost:7xxx/api` (replace xxx with your port)

## 🔐 Authentication

All endpoints except public ones require JWT Bearer token:
```
Authorization: Bearer <your-jwt-token>
```

---

## 📋 Authentication Endpoints

### 1. Request OTP
**POST** `/api/auth/request-otp`

Request body:
```json
{
  "mobileNumber": "1234567890",
  "purpose": "Registration"
}
```

### 2. Register User (Employer)
**POST** `/api/auth/register`

Request body:
```json
{
  "mobileNumber": "1234567890",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123",
  "otpCode": "123456"
}
```

### 3. Login
**POST** `/api/auth/login`

Request body:
```json
{
  "mobileNumber": "1234567890",
  "password": "SecurePassword123"
}
```

Response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "mobileNumber": "1234567890",
    "email": "user@example.com",
    "role": "Employer",
    "token": "jwt-token-here",
    "tokenExpiry": "2024-01-01T00:00:00Z",
    "isTemporaryPassword": false,
    "requirePasswordChange": false
  }
}
```

### 4. Change Password
**POST** `/api/auth/change-password` [🔒 Requires Auth]

Request body:
```json
{
  "currentPassword": "OldPassword123",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

---

## 👷 Labor Search & Management Endpoints

### 1. Search Labors
**GET** `/api/labor/search`

Query parameters:
- `laborTypeId` (optional): Filter by labor type
- `searchText` (optional): Search in name, specialization, labor type
- `latitude` (optional): GPS latitude for location search
- `longitude` (optional): GPS longitude for location search
- `radiusKm` (optional, default: 50): Search radius in kilometers
- `minRating` (optional, default: 0): Minimum rating filter
- `maxDailyRate` (optional): Maximum daily rate filter
- `availabilityStatus` (optional, default: "Available"): Availability filter
- `pageNumber` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Page size

Example:
```
GET /api/labor/search?laborTypeId=guid&latitude=12.9716&longitude=77.5946&radiusKm=10&minRating=4.0&pageNumber=1&pageSize=10
```

### 2. Get Labor Details
**GET** `/api/labor/{laborId}`

Response includes labor info, skills, and recent reviews.

### 3. Get Labor Types
**GET** `/api/labor/types`

Returns all available labor types (Normal, Plumber, Electrician, etc.)

### 4. Update Labor Availability
**PUT** `/api/labor/{laborId}/availability` [🔒 Labor/Admin Only]

Request body:
```json
{
  "availabilityStatus": "Available"
}
```

### 5. Add Labor Skill
**POST** `/api/labor/{laborId}/skills` [🔒 Labor/Admin Only]

Request body:
```json
{
  "skillName": "Pipe Installation",
  "proficiencyLevel": "Expert"
}
```

### 6. Remove Labor Skill
**DELETE** `/api/labor/skills/{skillId}` [🔒 Labor/Admin Only]

---

## 🛒 Cart Management Endpoints

### 1. Get Cart
**GET** `/api/cart` [🔒 Employer Only]

Returns cart items and summary.

### 2. Add to Cart
**POST** `/api/cart/add` [🔒 Employer Only]

Request body:
```json
{
  "laborId": "guid",
  "requiredHours": 4,
  "workDescription": "Plumbing repair work",
  "preferredDate": "2024-12-25T09:00:00Z"
}
```

### 3. Update Cart Item
**PUT** `/api/cart/{cartId}` [🔒 Employer Only]

Request body:
```json
{
  "requiredHours": 6,
  "workDescription": "Updated work description",
  "preferredDate": "2024-12-26T10:00:00Z"
}
```

### 4. Remove from Cart
**DELETE** `/api/cart/{cartId}` [🔒 Employer Only]

### 5. Clear Cart
**DELETE** `/api/cart/clear` [🔒 Employer Only]

### 6. Get Cart Item Count
**GET** `/api/cart/count` [🔒 Employer Only]

---

## 📦 Order Management Endpoints

### 1. Create Order from Cart
**POST** `/api/order` [🔒 Employer Only]

Request body:
```json
{
  "workAddressId": "guid",
  "scheduledDate": "2024-12-25T09:00:00Z",
  "specialInstructions": "Ring doorbell twice"
}
```

### 2. Get Order Details
**GET** `/api/order/{orderId}` [🔒 Requires Auth]

Returns complete order information including items and tracking.

### 3. Get My Orders
**GET** `/api/order` [🔒 Requires Auth]

Query parameters:
- `orderStatus` (optional): Filter by status
- `pageNumber` (optional, default: 1)
- `pageSize` (optional, default: 20)

Different behavior based on user role:
- **Employer**: Returns orders they created
- **Labor**: Returns orders where they are assigned
- **Admin**: Returns all orders

### 4. Update Order Status
**PUT** `/api/order/{orderId}/status` [🔒 Admin/Labor Only]

Request body:
```json
{
  "newStatus": "InProgress",
  "description": "Work has started"
}
```

### 5. Update Order Item Status
**PUT** `/api/order/items/{orderItemId}/status` [🔒 Labor/Admin Only]

Request body:
```json
{
  "itemStatus": "Completed",
  "actualHours": 5,
  "startTime": "2024-12-25T09:00:00Z",
  "endTime": "2024-12-25T14:00:00Z"
}
```

### 6. Add Order Tracking
**POST** `/api/order/{orderId}/tracking` [🔒 Admin/Labor Only]

Request body:
```json
{
  "status": "On the way",
  "description": "Labor is heading to work location",
  "location": "123 Main Street"
}
```

---

## 📍 Address Management Endpoints

### 1. Get My Addresses
**GET** `/api/address` [🔒 Requires Auth]

### 2. Get Address by ID
**GET** `/api/address/{addressId}` [🔒 Requires Auth]

### 3. Create Address
**POST** `/api/address` [🔒 Requires Auth]

Request body:
```json
{
  "addressType": "Home",
  "street": "123 Main Street, Apt 4B",
  "city": "New York",
  "state": "NY",
  "country": "USA",
  "zipCode": "10001",
  "latitude": 40.7589,
  "longitude": -73.9851,
  "isDefault": true
}
```

### 4. Update Address
**PUT** `/api/address/{addressId}` [🔒 Requires Auth]

### 5. Delete Address
**DELETE** `/api/address/{addressId}` [🔒 Requires Auth]

### 6. Set Default Address
**PUT** `/api/address/{addressId}/set-default` [🔒 Requires Auth]

### 7. Get Default Address
**GET** `/api/address/default` [🔒 Requires Auth]

---

## ⭐ Review Management Endpoints

### 1. Add Review
**POST** `/api/review` [🔒 Employer Only]

Request body:
```json
{
  "orderItemId": "guid",
  "laborId": "guid",
  "rating": 5,
  "comment": "Excellent work, very professional!"
}
```

### 2. Get Labor Reviews
**GET** `/api/review/labor/{laborId}`

Query parameters:
- `pageNumber` (optional, default: 1)
- `pageSize` (optional, default: 10)

### 3. Get Review by ID
**GET** `/api/review/{reviewId}` [🔒 Requires Auth]

### 4. Update Review
**PUT** `/api/review/{reviewId}` [🔒 Employer Only]

Request body:
```json
{
  "rating": 4,
  "comment": "Good work, minor issues"
}
```

### 5. Delete Review
**DELETE** `/api/review/{reviewId}` [🔒 Employer/Admin Only]

### 6. Get Labor Average Rating
**GET** `/api/review/labor/{laborId}/average-rating`

---

## 👨‍💼 Admin Endpoints

### 1. Onboard Labor
**POST** `/api/admin/onboard-labor` [🔒 Admin Only]

Request body:
```json
{
  "mobileNumber": "1234567890",
  "email": "labor@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "laborTypeId": "guid",
  "specialization": "Residential Plumbing",
  "experienceYears": 5,
  "dailyRate": 150.00,
  "minimumHours": 2,
  "maximumHours": 8
}
```

Response includes temporary password for the labor user.

### 2. Get All Users
**GET** `/api/admin/users` [🔒 Admin Only]

Query parameters:
- `role` (optional): Filter by role

### 3. Activate User
**PUT** `/api/admin/user/{userId}/activate` [🔒 Admin Only]

### 4. Deactivate User
**PUT** `/api/admin/user/{userId}/deactivate` [🔒 Admin Only]

### 5. Verify Labor
**PUT** `/api/admin/labor/{laborId}/verify` [🔒 Admin Only]

---

## 📊 Response Format

All API responses follow this structure:

### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* Response data */ },
  "errors": []
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message",
  "data": null,
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

---

## 🔑 User Roles & Permissions

### Admin
- Full system access
- Can onboard labor users
- Can manage all users and orders
- Can verify labor profiles

### Labor
- Can update their availability and skills
- Can view and update assigned orders
- Can add order tracking information
- Must change password on first login (if onboarded by admin)

### Employer
- Can search and book labor services
- Can manage cart and create orders
- Can add reviews for completed work
- Can manage their addresses
- Can register independently with OTP verification

---

## 🚀 Getting Started

1. **For Employers**:
   - Request OTP → Register → Login → Search Labor → Add to Cart → Create Order

2. **For Labor** (Admin Onboarded):
   - Login with temporary password → Change password → Update availability → Manage assigned orders

3. **For Admin**:
   - Login → Onboard labor users → Manage system

---

## 🔍 Search Examples

### Basic Labor Search
```
GET /api/labor/search?searchText=plumber
```

### Location-based Search
```
GET /api/labor/search?latitude=12.9716&longitude=77.5946&radiusKm=5
```

### Filtered Search
```
GET /api/labor/search?laborTypeId=guid&minRating=4.0&maxDailyRate=100&availabilityStatus=Available
```

## 📱 Status Values

### Order Status
- `Pending` - Order created, waiting confirmation
- `Confirmed` - Order confirmed by system/admin
- `InProgress` - Work in progress
- `Completed` - Work completed
- `Cancelled` - Order cancelled

### Payment Status
- `Pending` - Payment not processed
- `Paid` - Payment completed
- `Failed` - Payment failed
- `Refunded` - Payment refunded

### Availability Status
- `Available` - Labor available for work
- `Busy` - Labor currently busy
- `Unavailable` - Labor unavailable

---

## 🏗️ Database Setup

Before using the API, run these SQL scripts in order:
1. `Database/01_CreateDatabase.sql` - Creates database and tables
2. `Database/02_StoredProcedures.sql` - Creates stored procedures
3. `Database/03_Functions.sql` - Creates utility functions
4. `Database/04_InitialData.sql` - Inserts initial data

**Default Admin Credentials:**
- Mobile: 1234567890
- Password: admin123 (change after first login) 