-- Labor Management System Initial Data
USE LaborManagementDB;
GO

-- Insert Default Roles
INSERT INTO [User].[Roles] (RoleId, RoleName, Description) VALUES
(NEWID(), 'Admin', 'System Administrator with full access'),
(NEWID(), 'Labor', 'Labor users who provide services'),
(NEWID(), 'Employer', 'Employers who hire labor services');
GO

-- Insert Labor Types
INSERT INTO [Labor].[LaborTypes] (LaborTypeId, TypeName, Description, DailyRate) VALUES
(NEWID(), 'Normal', 'General labor work', 50.00),
(NEWID(), 'Plumber', 'Plumbing and water system services', 150.00),
(NEWID(), 'Electrician', 'Electrical work and installations', 200.00),
(NEWID(), 'Carpenter', 'Woodwork and carpentry services', 120.00),
(NEWID(), 'Painter', 'Painting and decoration services', 80.00),
(NEWID(), 'Cleaner', 'Cleaning and maintenance services', 60.00),
(NEWID(), 'Gardener', 'Gardening and landscaping services', 70.00);
GO

-- Get Role IDs for permissions
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [User].[Roles] WHERE RoleName = 'Admin');
DECLARE @LaborRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [User].[Roles] WHERE RoleName = 'Labor');
DECLARE @EmployerRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [User].[Roles] WHERE RoleName = 'Employer');

-- Admin Permissions (Full Access)
INSERT INTO [System].[RolePermissions] (RoleId, FeatureName, CanView, CanCreate, CanEdit, CanDelete) VALUES
(@AdminRoleId, 'Users', 1, 1, 1, 1),
(@AdminRoleId, 'Roles', 1, 1, 1, 1),
(@AdminRoleId, 'Labors', 1, 1, 1, 1),
(@AdminRoleId, 'LaborTypes', 1, 1, 1, 1),
(@AdminRoleId, 'Orders', 1, 1, 1, 1),
(@AdminRoleId, 'Reviews', 1, 1, 1, 1),
(@AdminRoleId, 'Reports', 1, 1, 1, 1),
(@AdminRoleId, 'Permissions', 1, 1, 1, 1);

-- Labor Permissions (Limited Access)
INSERT INTO [System].[RolePermissions] (RoleId, FeatureName, CanView, CanCreate, CanEdit, CanDelete) VALUES
(@LaborRoleId, 'Profile', 1, 0, 1, 0),
(@LaborRoleId, 'Orders', 1, 0, 1, 0),
(@LaborRoleId, 'Reviews', 1, 0, 0, 0);

-- Employer Permissions (Customer Access)
INSERT INTO [System].[RolePermissions] (RoleId, FeatureName, CanView, CanCreate, CanEdit, CanDelete) VALUES
(@EmployerRoleId, 'Profile', 1, 0, 1, 0),
(@EmployerRoleId, 'LaborSearch', 1, 0, 0, 0),
(@EmployerRoleId, 'Cart', 1, 1, 1, 1),
(@EmployerRoleId, 'Orders', 1, 1, 1, 0),
(@EmployerRoleId, 'Reviews', 1, 1, 1, 1),
(@EmployerRoleId, 'Addresses', 1, 1, 1, 1);
GO

-- Create default Admin user
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT RoleId FROM [User].[Roles] WHERE RoleName = 'Admin');

INSERT INTO [User].[Users] (UserId, MobileNumber, Email, FirstName, LastName, RoleId, PasswordHash, IsActive, IsMobileVerified, CreatedAt)
VALUES (
    NEWID(), 
    '1234567890', 
    'admin@labormanagement.com', 
    'System', 
    'Admin', 
    @AdminRoleId, 
    '$2a$11$YourHashedPasswordHere', -- You should hash 'admin123' or similar default password
    1, 
    1, 
    GETUTCDATE()
);
GO

PRINT 'Initial data inserted successfully!'
PRINT 'Default Admin credentials:'
PRINT 'Mobile: 1234567890'
PRINT 'Password: admin123 (Please change this after first login)'
PRINT ''
PRINT 'Available Labor Types:'
SELECT TypeName, DailyRate FROM [Labor].[LaborTypes] ORDER BY TypeName;
GO 