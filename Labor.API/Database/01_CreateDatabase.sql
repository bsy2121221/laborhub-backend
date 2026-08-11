-- Labor Management System Database Schema
-- Create Database
CREATE DATABASE LaborManagementDataBase;
GO

USE LaborManagementDataBase;
GO



-- Create Tables

-- User.Roles Table
CREATE TABLE [dbo].[Roles] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT
);
GO

-- User.Person Table
CREATE TABLE [dbo].[Person] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255),
    ProfilePicture NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT
);
GO

-- User.Users Table
CREATE TABLE [dbo].[Users] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100),
    PersonID INT,
    MobileNumber NVARCHAR(15) NOT NULL UNIQUE,
    RoleID INT NOT NULL,
    PasswordHash NVARCHAR(255), -- Optional for OTP-only users
    IsTemporaryPassword BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    IsMobileVerified BIT DEFAULT 0,
    IsEmailVerified BIT DEFAULT 0,
    IsProfileComplete BIT DEFAULT 0, -- New field to track profile completion
    LastLoginAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT,
    FOREIGN KEY (PersonID) REFERENCES [dbo].[Person](ID),
    FOREIGN KEY (RoleID) REFERENCES [dbo].[Roles](ID)
);
GO

-- User.Addresses Table
CREATE TABLE [dbo].[Addresses] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    AddressType NVARCHAR(50) NOT NULL, -- 'Home', 'Work', 'Billing', etc.
    Street NVARCHAR(255) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    ZipCode NVARCHAR(20) NOT NULL,
    Latitude DECIMAL(10, 8),
    Longitude DECIMAL(11, 8),
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (UserID) REFERENCES [dbo].[Users](ID)
);
GO

-- System.OTPVerifications Table
CREATE TABLE [dbo].[OTPVerifications] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    MobileNumber NVARCHAR(15) NOT NULL,
    OTPCode NVARCHAR(6) NOT NULL,
    Purpose NVARCHAR(50) NOT NULL, -- 'Registration', 'Login', 'PasswordReset'
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT DEFAULT 0,
    VerifiedAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- Labor.LaborTypes Table
CREATE TABLE [dbo].[LaborTypes] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    DailyRate DECIMAL(10, 2) DEFAULT 0,
    PerDayRate DECIMAL(10, 2) DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT
);
GO

-- Labor.Labors Table
CREATE TABLE [dbo].[Labors] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    LaborTypeID INT NOT NULL,
    Specialization NVARCHAR(255),
    ExperienceYears INT DEFAULT 0,
    Rating DECIMAL(3, 2) DEFAULT 0.00,
    TotalReviews INT DEFAULT 0,
    DailyRate DECIMAL(10, 2) NOT NULL,
    MinimumHours INT DEFAULT 1,
    MaximumHours INT DEFAULT 24,
    AvailabilityStatus NVARCHAR(50) DEFAULT 'Available', -- 'Available', 'Busy', 'Unavailable'
    IsVerified BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT,
    FOREIGN KEY (UserID) REFERENCES [dbo].[Users](ID),
    FOREIGN KEY (LaborTypeID) REFERENCES [dbo].[LaborTypes](ID)
);
GO

-- Labor.LaborSkills Table
CREATE TABLE [dbo].[LaborSkills] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    LaborID INT NOT NULL,
    SkillName NVARCHAR(100) NOT NULL,
    ProficiencyLevel NVARCHAR(50) NOT NULL, -- 'Beginner', 'Intermediate', 'Advanced', 'Expert'
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (LaborID) REFERENCES [dbo].[Labors](ID) ON DELETE CASCADE
);
GO

-- Order.Carts Table
CREATE TABLE [dbo].[Carts] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    EmployerID INT NOT NULL,
    LaborID INT NOT NULL,
    RequiredHours INT NOT NULL,
    DailyRate DECIMAL(10, 2) NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    WorkDescription NVARCHAR(1000),
    PreferredDate DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (EmployerID) REFERENCES [dbo].[Users](ID),
    FOREIGN KEY (LaborID) REFERENCES [dbo].[Labors](ID)
);
GO

-- Order.Orders Table
CREATE TABLE [dbo].[Orders] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    EmployerID INT NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    OrderStatus NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'
    PaymentStatus NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Paid', 'Failed', 'Refunded'
    WorkAddressID INT NOT NULL,
    ScheduledDate DATETIME2,
    CompletedDate DATETIME2,
    CancelledDate DATETIME2,
    CancelReason NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (EmployerID) REFERENCES [dbo].[Users](ID),
    FOREIGN KEY (WorkAddressID) REFERENCES [dbo].[Addresses](ID)
);
GO

-- Order.OrderItems Table
CREATE TABLE [dbo].[OrderItems] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    LaborID INT NOT NULL,
    RequiredHours INT NOT NULL,
    DailyRate DECIMAL(10, 2) NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    WorkDescription NVARCHAR(1000),
    ItemStatus NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Assigned', 'InProgress', 'Completed', 'Cancelled'
    ActualHours INT,
    StartTime DATETIME2,
    EndTime DATETIME2,
    FOREIGN KEY (OrderID) REFERENCES [dbo].[Orders](ID) ON DELETE CASCADE,
    FOREIGN KEY (LaborID) REFERENCES [dbo].[Labors](ID)
);
GO

-- Order.OrderTracking Table
CREATE TABLE [dbo].[OrderTracking] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    Location NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    FOREIGN KEY (OrderID) REFERENCES [dbo].[Orders](ID) ON DELETE CASCADE
);
GO

-- Labor.LaborReviews Table
CREATE TABLE [dbo].[LaborReviews] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderItemID INT NOT NULL,
    EmployerID INT NOT NULL,
    LaborID INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (OrderItemID) REFERENCES [dbo].[OrderItems](ID),
    FOREIGN KEY (EmployerID) REFERENCES [dbo].[Users](ID),
    FOREIGN KEY (LaborID) REFERENCES [dbo].[Labors](ID)
);
GO

-- System.RolePermissions Table
CREATE TABLE [dbo].[RolePermissions] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    FeatureName NVARCHAR(100) NOT NULL,
    CanView BIT DEFAULT 0,
    CanCreate BIT DEFAULT 0,
    CanEdit BIT DEFAULT 0,
    CanDelete BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy INT,
    UpdatedAt DATETIME2,
    UpdatedBy INT,
    FOREIGN KEY (RoleID) REFERENCES [dbo].[Roles](ID)
);
GO

-- Labor.LaborAvailabilities Table
CREATE TABLE [dbo].[LaborAvailabilities] (
  ID INT IDENTITY(1,1) PRIMARY KEY,
  LaborID INT NOT NULL,
  AvailableDate DATE NOT NULL,
  Status NVARCHAR(50) NOT NULL, -- e.g. 'Available', 'Unavailable', 'Busy'
  StartTime TIME NULL,
  EndTime TIME NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  UpdatedAt DATETIME2 NULL,
  CONSTRAINT FK_LaborAvailabilities_Labors
    FOREIGN KEY (LaborID) REFERENCES [dbo].[Labors](ID) ON DELETE CASCADE,
  CONSTRAINT UQ_LaborAvailabilities_Labor_Date UNIQUE (LaborID, AvailableDate)
);
GO

-- SP: Get Labor Availabilities by Month
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborAvailabilitiesByMonth]
    @LaborId INT,
    @Year INT,
    @Month INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATE = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @EndDate DATE = EOMONTH(@StartDate);

    SELECT
        AvailableDate,
        Status,
        StartTime,
        EndTime
    FROM [dbo].[LaborAvailabilities]
    WHERE LaborID = @LaborId
      AND AvailableDate BETWEEN @StartDate AND @EndDate
    ORDER BY AvailableDate;
END
GO
CREATE TABLE [dbo].[LaborConfirmations] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    OrderItemID INT NOT NULL,
    LaborID INT NOT NULL,
    LaborMobile NVARCHAR(20) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Pending', -- Pending, Accepted, Declined, NoResponse, Expired
    AttemptCount INT NOT NULL DEFAULT 0,
    NextCallAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastCallAt DATETIME2 NULL,
    LastCallProviderId NVARCHAR(100) NULL,
    ResponseDigit NVARCHAR(5) NULL,
    RespondedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_LaborConfirmations_Orders FOREIGN KEY (OrderID) REFERENCES [dbo].[Orders](ID),
    CONSTRAINT FK_LaborConfirmations_OrderItems FOREIGN KEY (OrderItemID) REFERENCES [dbo].[OrderItems](ID),
    CONSTRAINT FK_LaborConfirmations_Labors FOREIGN KEY (LaborID) REFERENCES [dbo].[Labors](ID),
    CONSTRAINT UQ_LaborConfirmations_OrderItem UNIQUE (OrderItemID)
);
GO
CREATE TABLE [dbo].[IvrCallLogs] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    LaborConfirmationID INT NOT NULL,
    ProviderCallId NVARCHAR(100) NULL,
    CallStatus NVARCHAR(50) NULL,
    DtmfDigit NVARCHAR(5) NULL,
    RawPayload NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_IvrCallLogs_LaborConfirmations FOREIGN KEY (LaborConfirmationID) REFERENCES [dbo].[LaborConfirmations](ID)
);
GO
CREATE TABLE [dbo].[NotificationLogs] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL,
    Mobile NVARCHAR(20) NOT NULL,
    Channel NVARCHAR(20) NOT NULL, -- SMS, WhatsApp
    TemplateKey NVARCHAR(100) NOT NULL,
    MessageBody NVARCHAR(2000) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Sent, Failed, Mocked
    ProviderMessageId NVARCHAR(100) NULL,
    ErrorMessage NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO
CREATE TABLE [dbo].[Payments] (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    EmployerID INT NOT NULL,
    LaborAmount DECIMAL(10,2) NOT NULL,
    PlatformFee DECIMAL(10,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
    CouponCode NVARCHAR(50) NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT N'INR',
    Provider NVARCHAR(30) NOT NULL DEFAULT N'Mock', -- Mock, Razorpay
    ProviderOrderId NVARCHAR(100) NULL,
    ProviderPaymentId NVARCHAR(100) NULL,
    ProviderSignature NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT N'Created', -- Created, Paid, Failed, Refunded
    FailureReason NVARCHAR(500) NULL,
    PaidAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderID) REFERENCES [dbo].[Orders](ID),
    CONSTRAINT FK_Payments_Employer FOREIGN KEY (EmployerID) REFERENCES [dbo].[Users](ID)
);
GO


-- Create Indexes for Performance
CREATE INDEX IX_Users_MobileNumber ON [dbo].[Users](MobileNumber);
CREATE INDEX IX_Users_RoleID ON [dbo].[Users](RoleID);
CREATE INDEX IX_Users_PersonID ON [dbo].[Users](PersonID);
CREATE INDEX IX_Addresses_UserID ON [dbo].[Addresses](UserID);
CREATE INDEX IX_Labors_UserID ON [dbo].[Labors](UserID);
CREATE INDEX IX_Labors_LaborTypeID ON [dbo].[Labors](LaborTypeID);
CREATE INDEX IX_Labors_AvailabilityStatus ON [dbo].[Labors](AvailabilityStatus);
CREATE INDEX IX_Orders_EmployerID ON [dbo].[Orders](EmployerID);
CREATE INDEX IX_Orders_OrderStatus ON [dbo].[Orders](OrderStatus);
CREATE INDEX IX_OrderItems_OrderID ON [dbo].[OrderItems](OrderID);
CREATE INDEX IX_OrderItems_LaborID ON [dbo].[OrderItems](LaborID);
CREATE INDEX IX_Carts_EmployerID ON [dbo].[Carts](EmployerID);
CREATE INDEX IX_OTPVerifications_MobileNumber ON [dbo].[OTPVerifications](MobileNumber);
CREATE INDEX IX_OrderTracking_OrderID ON [dbo].[OrderTracking](OrderID);
CREATE INDEX IX_Payments_OrderID ON [dbo].[Payments](OrderID);
CREATE INDEX IX_Payments_EmployerID ON [dbo].[Payments](EmployerID);
CREATE INDEX IX_Payments_Status ON [dbo].[Payments](Status);
GO 