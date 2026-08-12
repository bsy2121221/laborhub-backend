-- Labor Management System Stored Procedures
USE LaborManagementDataBase;
GO

-- =============================================
-- User profile update with optional password
-- =============================================
USE [LaborManagementDataBase_new]
GO
/****** Object:  StoredProcedure [dbo].[sp_UpdateUserProfile]    Script Date: 22-06-2026 13:35:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[sp_UpdateUserProfile]
    @LoginUserId INT,
    @TargetUserId INT,
    @UserName NVARCHAR(100) = NULL,
    @MobileNumber NVARCHAR(15) = NULL,
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @Email NVARCHAR(255) = NULL,
    @ProfilePicture NVARCHAR(500) = NULL,
	@PasswordHash NVARCHAR(255) = NULL,
    @UpdatedBy INT = NULL
AS
BEGIN
     SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @CanEditUsers BIT = 0;

        SELECT TOP 1 @CanEditUsers = 1
        FROM [dbo].[Users] u
        INNER JOIN [dbo].[RolePermissions] rp ON rp.RoleID = u.RoleID
        WHERE u.ID = @LoginUserId
          AND rp.IsActive = 1
          AND rp.CanEdit = 1;

        IF (@LoginUserId <> @TargetUserId AND ISNULL(@CanEditUsers, 0) = 0)
        BEGIN
            RAISERROR('You are not allowed to update this user.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

    IF @MobileNumber IS NOT NULL AND EXISTS (
        SELECT 1 FROM [dbo].[Users] WHERE [MobileNumber] = @MobileNumber AND [ID] <> @TargetUserId
    )
    BEGIN
        RAISERROR('Mobile number already in use.', 16, 1);
        RETURN;
    END

    DECLARE @PersonId INT;
    SELECT @PersonId = [PersonID] FROM [dbo].[Users] WHERE [ID] = @TargetUserId;

    IF @PersonId IS NULL
    BEGIN
        RAISERROR('User has no person record.', 16, 1);
        RETURN;
    END

    UPDATE u
        SET
            u.UserName = COALESCE(NULLIF(@UserName, N''), u.UserName),
            u.MobileNumber = COALESCE(NULLIF(@MobileNumber, N''), u.MobileNumber),
            u.PasswordHash = CASE WHEN @PasswordHash IS NOT NULL THEN @PasswordHash ELSE u.PasswordHash END,
            u.UpdatedAt = GETUTCDATE(),
            u.UpdatedBy = @UpdatedBy
        FROM [dbo].[Users] u
        WHERE u.ID = @TargetUserId;

    UPDATE [dbo].[Person]
    SET
        [FirstName] = COALESCE(@FirstName, [FirstName]),
        [LastName] = COALESCE(@LastName, [LastName]),
        [Email] = COALESCE(@Email, [Email]),
        [ProfilePicture] = COALESCE(@ProfilePicture, [ProfilePicture]),
        [UpdatedAt] = GETDATE(),
        [UpdatedBy] = @UpdatedBy
    WHERE [ID] = @PersonId;

    COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS RowsAffected;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO

-- =============================================
-- User Management Stored Procedures
-- =============================================

-- SP: Create User
CREATE PROCEDURE [dbo].[sp_CreateUser]
    @MobileNumber NVARCHAR(15),
    @Email NVARCHAR(255) = NULL,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @RoleID INT,
    @PasswordHash NVARCHAR(255),
    @IsTemporaryPassword BIT = 0,
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @PersonID INT;
    DECLARE @UserID INT;
    
    -- First create the Person record
    INSERT INTO [dbo].[Person] (
        FirstName, LastName, CreatedBy
    )
    VALUES (
        @FirstName, @LastName, @CreatedBy
    );
    
    SET @PersonID = SCOPE_IDENTITY();
    
    -- Then create the User record
    INSERT INTO [dbo].[Users] (
        PersonID, MobileNumber, Email, RoleID, 
        PasswordHash, IsTemporaryPassword, CreatedBy
    )
    VALUES (
        @PersonID, @MobileNumber, @Email, @RoleID,
        @PasswordHash, @IsTemporaryPassword, @CreatedBy
    );
    
    SET @UserID = SCOPE_IDENTITY();
    
    COMMIT TRANSACTION;
    
    SELECT @UserID AS UserID, @PersonID AS PersonID;
END
GO

-- SP: Get User by Mobile Number
CREATE PROCEDURE [dbo].[sp_GetUserByMobileNumber]
    @MobileNumber NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.ID as UserID, u.PersonID, u.MobileNumber, u.Email, 
        p.FirstName, p.LastName, p.ProfilePicture,
        u.RoleID, r.RoleName, u.PasswordHash,
        u.IsTemporaryPassword, u.IsActive, u.IsMobileVerified,
        u.IsEmailVerified, u.LastLoginAt, u.CreatedAt
    FROM [dbo].[Users] u
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[Roles] r ON u.RoleID = r.ID
    WHERE u.MobileNumber = @MobileNumber AND u.IsActive = 1;
END
GO

-- SP: Update User Password
CREATE PROCEDURE [dbo].[sp_UpdateUserPassword]
    @UserID INT,
    @NewPasswordHash NVARCHAR(255),
    @IsTemporaryPassword BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Users]
    SET PasswordHash = @NewPasswordHash,
        IsTemporaryPassword = @IsTemporaryPassword,
        UpdatedAt = GETUTCDATE()
    WHERE ID = @UserID;
END
GO

-- SP: Update Last Login
CREATE PROCEDURE [dbo].[sp_UpdateLastLogin]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Users]
    SET LastLoginAt = GETUTCDATE()
    WHERE ID = @UserID;
END
GO

-- =============================================
-- OTP Management Stored Procedures
-- =============================================

-- SP: Create OTP
CREATE PROCEDURE [dbo].[sp_CreateOTP]
    @MobileNumber NVARCHAR(15),
    @OTPCode NVARCHAR(6),
    @Purpose NVARCHAR(50),
    @ExpiryMinutes INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Invalidate existing OTPs for this mobile number and purpose
    UPDATE [dbo].[OTPVerifications]
    SET IsUsed = 1
    WHERE MobileNumber = @MobileNumber AND Purpose = @Purpose AND IsUsed = 0;
    
    -- Create new OTP
    INSERT INTO [dbo].[OTPVerifications] (
        MobileNumber, OTPCode, Purpose, ExpiresAt
    )
    VALUES (
        @MobileNumber, @OTPCode, @Purpose, DATEADD(MINUTE, @ExpiryMinutes, GETUTCDATE())
    );
    
    SELECT SCOPE_IDENTITY() AS OTPID;
END
GO

-- SP: Verify OTP
CREATE PROCEDURE [dbo].[sp_VerifyOTP]
    @MobileNumber NVARCHAR(15),
    @OTPCode NVARCHAR(6),
    @Purpose NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsValid BIT = 0;
    DECLARE @OTPID INT;
    
    SELECT @OTPID = ID
    FROM [dbo].[OTPVerifications]
    WHERE MobileNumber = @MobileNumber 
      AND OTPCode = @OTPCode 
      AND Purpose = @Purpose
      AND IsUsed = 0
      AND ExpiresAt > GETUTCDATE();
    
    IF @OTPID IS NOT NULL
    BEGIN
        SET @IsValid = 1;
        UPDATE [dbo].[OTPVerifications]
        SET IsUsed = 1, VerifiedAt = GETUTCDATE()
        WHERE ID = @OTPID;
    END
    
    SELECT @IsValid AS IsValid;
END
GO

-- =============================================
-- Labor Management Stored Procedures
-- =============================================

-- SP: Create Labor Profile
CREATE PROCEDURE [dbo].[sp_CreateLaborProfile]
    @UserID INT,
    @LaborTypeID INT,
    @Specialization NVARCHAR(255) = NULL,
    @ExperienceYears INT = 0,
    @DailyRate DECIMAL(10, 2),
    @MinimumHours INT = 1,
    @MaximumHours INT = 24,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[Labors] (
        UserID, LaborTypeID, Specialization, ExperienceYears,
        DailyRate, MinimumHours, MaximumHours, CreatedBy
    )
    VALUES (
        @UserID, @LaborTypeID, @Specialization, @ExperienceYears,
        @DailyRate, @MinimumHours, @MaximumHours, @CreatedBy
    );
    
    SELECT SCOPE_IDENTITY() AS LaborID;
END
GO

-- SP: Search Labors with Location Filter
CREATE PROCEDURE [dbo].[sp_SearchLabors]
    @LaborTypeId INT = NULL,
    @SearchText NVARCHAR(255) = NULL,
    @AvailabilityStatus NVARCHAR(50) = NULL,
    @MinRating DECIMAL(3,2) = 0,
    @MaxDailyRate DECIMAL(10,2) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @Today DATE = CAST(GETUTCDATE() AS DATE);

    SELECT
        l.ID AS LaborId,
        l.UserID AS UserId,
        p.FirstName,
        p.LastName,
        p.ProfilePicture,
        l.LaborTypeID,
        lt.TypeName AS LaborType,
        l.Specialization,
        l.ExperienceYears,
        l.Rating,
        l.TotalReviews,
        l.DailyRate,
        l.MinimumHours,
        l.MaximumHours,
        l.AvailabilityStatus,
        l.IsVerified,
        a.City,
        a.State,
        la.AvailableDate,
        CONVERT(VARCHAR(8), la.StartTime, 108) AS StartTime,
        CONVERT(VARCHAR(8), la.EndTime, 108) AS EndTime
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    LEFT JOIN [dbo].[Addresses] a ON u.ID = a.UserID AND a.IsDefault = 1
    INNER JOIN (
        SELECT
            LaborID,
            AvailableDate,
            StartTime,
            EndTime,
            ROW_NUMBER() OVER (
                PARTITION BY LaborID
                ORDER BY AvailableDate, StartTime
            ) AS rn
        FROM [dbo].[LaborAvailabilities]
        WHERE AvailableDate >= @Today
          AND Status = N'Available'
    ) la ON la.LaborID = l.ID AND la.rn = 1
    WHERE l.IsActive = 1
      AND u.IsActive = 1
      AND (@LaborTypeId IS NULL OR l.LaborTypeID = @LaborTypeId)
      AND (
          @SearchText IS NULL
          OR (
              p.FirstName LIKE N'%' + @SearchText + N'%'
              OR p.LastName LIKE N'%' + @SearchText + N'%'
              OR l.Specialization LIKE N'%' + @SearchText + N'%'
          )
      )
      AND (@AvailabilityStatus IS NULL OR l.AvailabilityStatus = @AvailabilityStatus)
      AND (l.Rating >= @MinRating)
      AND (@MaxDailyRate IS NULL OR l.DailyRate <= @MaxDailyRate)
    ORDER BY l.Rating DESC, l.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: Get Labor Details
CREATE PROCEDURE [dbo].[sp_GetLaborDetails]
    @LaborID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Labor basic info
    SELECT 
        l.ID as LaborID, l.UserID, p.FirstName, p.LastName, u.MobileNumber, u.Email,
        p.ProfilePicture, l.LaborTypeID, lt.TypeName AS LaborType,
        l.Specialization, l.ExperienceYears, l.Rating, l.TotalReviews,
        l.DailyRate, l.MinimumHours, l.MaximumHours, l.AvailabilityStatus,
        l.IsVerified, l.IsActive, l.CreatedAt
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    WHERE l.ID = @LaborID;
    
    -- Labor skills
    SELECT ID as SkillID, SkillName, ProficiencyLevel
    FROM [dbo].[LaborSkills]
    WHERE LaborID = @LaborID;
    
    -- Recent reviews
    SELECT TOP 10
        lr.ID as ReviewID, lr.Rating, lr.Comment, lr.CreatedAt,
        p.FirstName + ' ' + p.LastName AS EmployerName
    FROM [dbo].[LaborReviews] lr
    INNER JOIN [dbo].[Users] u ON lr.EmployerID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE lr.LaborID = @LaborID AND lr.IsActive = 1
    ORDER BY lr.CreatedAt DESC;
END
GO

-- =============================================
-- Cart Management Stored Procedures
-- =============================================

-- SP: Add to Cart
CREATE PROCEDURE [dbo].[sp_AddToCart]
    @EmployerID INT,
    @LaborID INT,
    @RequiredHours INT,
    @WorkDescription NVARCHAR(1000) = NULL,
    @PreferredDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DailyRate DECIMAL(10, 2);
    DECLARE @TotalAmount DECIMAL(10, 2);
    DECLARE @CartID INT;    
    
    -- Get labor daily rate
    SELECT @DailyRate = DailyRate
    FROM [dbo].[Labors]
    WHERE ID = @LaborID AND IsActive = 1;
    
    IF @DailyRate IS NULL
    BEGIN
        RAISERROR('Labor not found or inactive', 16, 1);
        RETURN;
    END
    
    SET @TotalAmount = @DailyRate * @RequiredHours;
    
    -- Check if item already exists in cart
    SELECT @CartID = ID
    FROM [dbo].[Carts]
    WHERE EmployerID = @EmployerID AND LaborID = @LaborID;
    
    IF @CartID IS NOT NULL
    BEGIN
        -- Update existing cart item
        UPDATE [dbo].[Carts]
        SET RequiredHours = @RequiredHours,
            DailyRate = @DailyRate,
            TotalAmount = @TotalAmount,
            WorkDescription = @WorkDescription,
            PreferredDate = @PreferredDate
        WHERE ID = @CartID;
    END
    ELSE
    BEGIN
        -- Add new cart item
        INSERT INTO [dbo].[Carts] (
            EmployerID, LaborID, RequiredHours, DailyRate,
            TotalAmount, WorkDescription, PreferredDate
        )
        VALUES (
            @EmployerID, @LaborID, @RequiredHours, @DailyRate,
            @TotalAmount, @WorkDescription, @PreferredDate
        );
        
        SET @CartID = SCOPE_IDENTITY();
    END
    
    SELECT @CartID AS CartID;
END
GO

-- SP: Get Cart Items
CREATE PROCEDURE [dbo].[sp_GetCartItems]
    @EmployerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ID as CartID, c.LaborID, c.RequiredHours, c.DailyRate, c.TotalAmount,
        c.WorkDescription, c.PreferredDate, c.CreatedAt,
        p.FirstName + ' ' + p.LastName AS LaborName,
        p.ProfilePicture, lt.TypeName AS LaborType,
        l.Specialization, l.Rating, l.TotalReviews
    FROM [dbo].[Carts] c
    INNER JOIN [dbo].[Labors] l ON c.LaborID = l.ID
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    WHERE c.EmployerID = @EmployerID
    ORDER BY c.CreatedAt DESC;
    
    -- Cart summary
    SELECT 
        COUNT(*) AS TotalItems,
        SUM(TotalAmount) AS TotalAmount
    FROM [dbo].[Carts]
    WHERE EmployerID = @EmployerID;
END
GO

-- SP: Remove from Cart
CREATE PROCEDURE [dbo].[sp_RemoveFromCart]
    @CartID INT,
    @EmployerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[Carts]
    WHERE ID = @CartID AND EmployerID = @EmployerID;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Order Management Stored Procedures
-- =============================================

-- SP: Create Order from Cart
ALTER   PROCEDURE [dbo].[sp_CreateOrderFromCart]
    @EmployerID INT,
    @WorkAddressID INT,
    @ScheduledDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
	
    
    
    DECLARE @OrderID INT;
    DECLARE @OrderNumber NVARCHAR(50);
    DECLARE @TotalAmount DECIMAL(10, 2);
    
    -- Generate order number
    DECLARE @OrderCount INT;
    SELECT @OrderCount = COUNT(*) + 1 FROM [dbo].[Orders];
    SET @OrderNumber = 'ORD' + FORMAT(@OrderCount, '000000');
    
    -- Calculate total amount
    SELECT @TotalAmount = SUM(TotalAmount)
    FROM [dbo].[Carts]
    WHERE EmployerID = @EmployerID;
    
    IF @TotalAmount IS NULL OR @TotalAmount = 0
    BEGIN
        RAISERROR('Cart is empty', 16, 1);
        RETURN;
    END
    begin try
	 BEGIN TRANSACTION;
	 -- If any cart item is invalid (not available), then block checkout
		IF EXISTS (
			SELECT 1
			FROM [dbo].[Carts] c
			WHERE c.EmployerID = @EmployerID
			  AND (
					c.PreferredDate IS NULL
					OR NOT EXISTS (
						SELECT 1
						FROM [dbo].[LaborAvailabilities] la
						WHERE la.LaborID = c.LaborID
						  AND la.AvailableDate = CAST(c.PreferredDate AS DATE)
						  AND la.Status = N'Available'
					)
				  )
		)
		BEGIN
			RAISERROR('One or more laborers are not available on selected date.', 16, 1);
		END
	
    -- Create order
    INSERT INTO [dbo].[Orders] (
        OrderNumber, EmployerID, TotalAmount, WorkAddressID, ScheduledDate,OrderStatus
    )
    VALUES (
        @OrderNumber, @EmployerID, @TotalAmount, @WorkAddressID, @ScheduledDate,N'Pending'
    );
    
    SET @OrderID = SCOPE_IDENTITY();
    
    -- Create order items from cart
    INSERT INTO [dbo].[OrderItems] (
        OrderID, LaborID, RequiredHours, DailyRate, TotalAmount, WorkDescription,ItemStatus,PreferredWorkDate
    )
    SELECT 
        @OrderID, LaborID, RequiredHours, DailyRate, TotalAmount, WorkDescription,N'PendingConfirmation',CAST(PreferredDate AS DATE)
    FROM [dbo].[Carts]
    WHERE EmployerID = @EmployerID;

	UPDATE la
	SET la.Status = N'OnHold',
		la.UpdatedAt = GETDATE()
	FROM [dbo].[LaborAvailabilities] la
	INNER JOIN [dbo].[Carts] c
		ON c.LaborID = la.LaborID
	   AND CAST(c.PreferredDate AS DATE) = cast(la.AvailableDate as date)
	WHERE c.EmployerID = @EmployerID
	  AND la.Status = N'Available'

	   -- Create IVR confirmation rows
    INSERT INTO [dbo].[LaborConfirmations] (OrderID, OrderItemID, LaborID, LaborMobile, Status, NextCallAt)
    SELECT @OrderID, oi.ID, oi.LaborID, u.MobileNumber, N'Pending', GETUTCDATE()
    FROM [dbo].[OrderItems] oi
    INNER JOIN [dbo].[Labors] l ON oi.LaborID = l.ID
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    WHERE oi.OrderID = @OrderID;
    
    -- Clear cart
    DELETE FROM [dbo].[Carts] WHERE EmployerID = @EmployerID;
    
    -- Add initial tracking
    INSERT INTO [dbo].[OrderTracking] (OrderID, Status, Description, CreatedBy)
    VALUES (@OrderID, N'Pending', N'Waiting for labor phone confirmation', @EmployerID);

		
    
    COMMIT TRANSACTION;
    
    SELECT @OrderID AS OrderID, @OrderNumber AS OrderNumber;
	end try
	BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END

    DECLARE @ErrMsg NVARCHAR(4000);
    SET @ErrMsg = ERROR_MESSAGE();

    RAISERROR(@ErrMsg, 16, 1);
    RETURN;
END CATCH
END
GO

-- ========== Recalculate order status from items ==========
CREATE OR ALTER PROCEDURE [dbo].[sp_RecalculateOrderConfirmationStatus]
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Total INT, @Accepted INT, @Declined INT, @Pending INT;
    SELECT
        @Total = COUNT(*),
        @Accepted = SUM(CASE WHEN ItemStatus = N'Assigned' THEN 1 ELSE 0 END),
        @Declined = SUM(CASE WHEN ItemStatus IN (N'Declined', N'Cancelled') THEN 1 ELSE 0 END),
        @Pending = SUM(CASE WHEN ItemStatus = N'PendingConfirmation' THEN 1 ELSE 0 END)
    FROM [dbo].[OrderItems] WHERE OrderID = @OrderID;
    DECLARE @NewStatus NVARCHAR(50);
    IF @Accepted = @Total SET @NewStatus = N'Confirmed';
    ELSE IF @Accepted > 0 AND @Pending > 0 SET @NewStatus = N'PartiallyConfirmed';
    ELSE IF @Accepted > 0 AND @Pending = 0 SET @NewStatus = N'PartiallyConfirmed';
    ELSE IF @Declined = @Total SET @NewStatus = N'Cancelled';
    ELSE SET @NewStatus = N'Pending';
    UPDATE [dbo].[Orders]
    SET OrderStatus = @NewStatus,
        UpdatedAt = GETUTCDATE(),
        CompletedDate = CASE WHEN @NewStatus = N'Confirmed' AND CompletedDate IS NULL THEN NULL ELSE CompletedDate END,
        CancelledDate = CASE WHEN @NewStatus = N'Cancelled' THEN GETUTCDATE() ELSE CancelledDate END
    WHERE ID = @OrderID;
    SELECT @NewStatus AS OrderStatus, @Total AS TotalLabor, @Accepted AS ConfirmedLabor, @Declined AS DeclinedLabor, @Pending AS PendingLabor;
END
GO

-- Recalculate order status after work starts / completes / labour backs out
CREATE OR ALTER PROCEDURE [dbo].[sp_RecalculateOrderWorkStatus]
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Total INT = 0;
    DECLARE @PendingConfirmation INT = 0;
    DECLARE @Declined INT = 0;
    DECLARE @Cancelled INT = 0;
    DECLARE @Assigned INT = 0;
    DECLARE @InProgress INT = 0;
    DECLARE @Completed INT = 0;
    DECLARE @Active INT = 0;
    DECLARE @Terminal INT = 0;
    DECLARE @NewStatus NVARCHAR(50);

    SELECT
        @Total = COUNT(*),
        @PendingConfirmation = SUM(CASE WHEN ItemStatus = N'PendingConfirmation' THEN 1 ELSE 0 END),
        @Declined = SUM(CASE WHEN ItemStatus = N'Declined' THEN 1 ELSE 0 END),
        @Cancelled = SUM(CASE WHEN ItemStatus = N'Cancelled' THEN 1 ELSE 0 END),
        @Assigned = SUM(CASE WHEN ItemStatus = N'Assigned' THEN 1 ELSE 0 END),
        @InProgress = SUM(CASE WHEN ItemStatus = N'InProgress' THEN 1 ELSE 0 END),
        @Completed = SUM(CASE WHEN ItemStatus = N'Completed' THEN 1 ELSE 0 END)
    FROM [dbo].[OrderItems]
    WHERE OrderID = @OrderID;

    IF @PendingConfirmation > 0
    BEGIN
        EXEC [dbo].[sp_RecalculateOrderConfirmationStatus] @OrderID;
        RETURN;
    END

    SET @Terminal = @Declined + @Cancelled;
    SET @Active = @Total - @Terminal;

    IF @Total = @Terminal
        SET @NewStatus = N'Cancelled';
    ELSE IF @Active > 0 AND @Completed = @Active
        SET @NewStatus = N'Completed';
    ELSE IF @InProgress > 0 OR (@Completed > 0 AND @Assigned > 0)
        SET @NewStatus = N'InProgress';
    ELSE IF @Assigned = @Active
        SET @NewStatus = N'Confirmed';
    ELSE
        SET @NewStatus = N'InProgress';

    IF @NewStatus = N'Completed'
    BEGIN
        UPDATE [dbo].[Orders]
        SET OrderStatus = N'Completed',
            CompletedDate = GETUTCDATE(),
            UpdatedAt = GETUTCDATE()
        WHERE ID = @OrderID;
    END
    ELSE IF @NewStatus = N'Cancelled'
    BEGIN
        UPDATE [dbo].[Orders]
        SET OrderStatus = N'Cancelled',
            CancelledDate = GETUTCDATE(),
            UpdatedAt = GETUTCDATE()
        WHERE ID = @OrderID;
    END
    ELSE IF @NewStatus = N'InProgress'
    BEGIN
        UPDATE [dbo].[Orders]
        SET OrderStatus = N'InProgress',
            UpdatedAt = GETUTCDATE()
        WHERE ID = @OrderID
          AND OrderStatus NOT IN (N'Completed', N'Cancelled');
    END
    ELSE IF @NewStatus = N'Confirmed'
    BEGIN
        UPDATE [dbo].[Orders]
        SET OrderStatus = N'Confirmed',
            UpdatedAt = GETUTCDATE()
        WHERE ID = @OrderID
          AND OrderStatus NOT IN (N'Completed', N'Cancelled');
    END
END
GO

-- SP: Get Order Details
CREATE PROCEDURE [dbo].[sp_GetOrderDetails]
    @OrderID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Order details
    SELECT 
        o.ID as OrderID, o.OrderNumber, o.EmployerID, o.TotalAmount,
        o.OrderStatus, o.PaymentStatus, o.ScheduledDate,
        o.CompletedDate, o.CancelledDate, o.CancelReason, o.CreatedAt,
        p.FirstName + ' ' + p.LastName AS EmployerName,
        u.MobileNumber AS EmployerMobile,
        a.Street, a.City, a.State, a.Country, a.ZipCode
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[Users] u ON o.EmployerID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[Addresses] a ON o.WorkAddressID = a.ID
    WHERE o.ID = @OrderID
      AND (
            @UserID IS NULL
            OR o.EmployerID = @UserID
            OR EXISTS (
                SELECT 1
                FROM [dbo].[OrderItems] oi2
                INNER JOIN [dbo].[Labors] l ON oi2.LaborID = l.ID
                WHERE oi2.OrderID = o.ID
                  AND l.UserID = @UserID
            )
          );
    
    -- Order items
    SELECT 
        oi.ID as OrderItemID, oi.LaborID, oi.RequiredHours, oi.DailyRate,
        oi.TotalAmount, oi.WorkDescription, oi.ItemStatus,
        oi.ActualHours, oi.StartTime, oi.EndTime,
        p.FirstName + ' ' + p.LastName AS LaborName,
        u.MobileNumber AS LaborMobile, p.ProfilePicture,
        lt.TypeName AS LaborType, l.Specialization, l.Rating
    FROM [dbo].[OrderItems] oi
    INNER JOIN [dbo].[Labors] l ON oi.LaborID = l.ID
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    WHERE oi.OrderID = @OrderID;
    
    -- Order tracking
    SELECT ID as TrackingID, Status, Description, Location, CreatedAt
    FROM [dbo].[OrderTracking]
    WHERE OrderID = @OrderID
    ORDER BY CreatedAt ASC;
END
GO

-- SP: Update Order Status
CREATE PROCEDURE [dbo].[sp_UpdateOrderStatus]
    @OrderID INT,
    @NewStatus NVARCHAR(50),
    @Description NVARCHAR(500) = NULL,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    -- Update order status
    UPDATE [dbo].[Orders]
    SET OrderStatus = @NewStatus,
        UpdatedAt = GETUTCDATE(),
        CompletedDate = CASE WHEN @NewStatus = 'Completed' THEN GETUTCDATE() ELSE CompletedDate END,
        CancelledDate = CASE WHEN @NewStatus = 'Cancelled' THEN GETUTCDATE() ELSE CancelledDate END
    WHERE ID = @OrderID;
    
    -- Add tracking entry
    INSERT INTO [dbo].[OrderTracking] (OrderID, Status, Description, CreatedBy)
    VALUES (@OrderID, @NewStatus, @Description, @UpdatedBy);
    
    COMMIT TRANSACTION;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Get User Orders
CREATE PROCEDURE [dbo].[sp_GetUserOrders]
    @UserID INT,
    @OrderStatus NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        o.ID as OrderID, o.OrderNumber, o.TotalAmount, o.OrderStatus,
        o.PaymentStatus, o.ScheduledDate, o.CreatedAt,
        COUNT(oi.ID) AS TotalItems,
        a.City + ', ' + a.State AS WorkLocation
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[OrderItems] oi ON o.ID = oi.OrderID
    INNER JOIN [dbo].[Addresses] a ON o.WorkAddressID = a.ID
    WHERE o.EmployerID = @UserID
      AND (@OrderStatus IS NULL OR o.OrderStatus = @OrderStatus)
    GROUP BY 
        o.ID, o.OrderNumber, o.TotalAmount, o.OrderStatus,
        o.PaymentStatus, o.ScheduledDate, o.CreatedAt,
        a.City, a.State
    ORDER BY o.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: Get Labor Work Assignments (jobs assigned to logged-in labor user)
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborOrders]
    @UserID INT,
    @OrderStatus NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LaborID INT;
    SELECT @LaborID = ID FROM [dbo].[Labors] WHERE UserID = @UserID;

    IF @LaborID IS NULL
    BEGIN
        SELECT
            CAST(NULL AS INT) AS OrderID,
            CAST(NULL AS NVARCHAR(50)) AS OrderNumber,
            CAST(NULL AS NVARCHAR(50)) AS OrderStatus,
            CAST(NULL AS NVARCHAR(50)) AS PaymentStatus,
            CAST(NULL AS DATETIME2) AS ScheduledDate,
            CAST(NULL AS DATETIME2) AS CreatedAt,
            CAST(NULL AS INT) AS OrderItemID,
            CAST(NULL AS INT) AS RequiredHours,
            CAST(NULL AS DECIMAL(10, 2)) AS DailyRate,
            CAST(NULL AS DECIMAL(10, 2)) AS ItemTotal,
            CAST(NULL AS NVARCHAR(1000)) AS WorkDescription,
            CAST(NULL AS NVARCHAR(50)) AS ItemStatus,
            CAST(NULL AS NVARCHAR(201)) AS EmployerName,
            CAST(NULL AS NVARCHAR(20)) AS EmployerMobile,
            CAST(NULL AS NVARCHAR(255)) AS Street,
            CAST(NULL AS NVARCHAR(511)) AS WorkLocation
        WHERE 1 = 0;
        RETURN;
    END

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT
        o.ID AS OrderID,
        o.OrderNumber,
        o.OrderStatus,
        o.PaymentStatus,
        o.ScheduledDate,
        o.CreatedAt,
        oi.ID AS OrderItemID,
        oi.RequiredHours,
        oi.DailyRate,
        oi.TotalAmount AS ItemTotal,
        oi.WorkDescription,
        oi.ItemStatus,
        p.FirstName + ' ' + p.LastName AS EmployerName,
        u.MobileNumber AS EmployerMobile,
        a.Street,
        a.City + N', ' + a.State AS WorkLocation
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[OrderItems] oi ON o.ID = oi.OrderID
    INNER JOIN [dbo].[Users] eu ON o.EmployerID = eu.ID
    INNER JOIN [dbo].[Person] p ON eu.PersonID = p.ID
    INNER JOIN [dbo].[Users] u ON o.EmployerID = u.ID
    INNER JOIN [dbo].[Addresses] a ON o.WorkAddressID = a.ID
    WHERE oi.LaborID = @LaborID
      AND (@OrderStatus IS NULL OR o.OrderStatus = @OrderStatus)
    ORDER BY
        CASE WHEN o.ScheduledDate IS NULL THEN 1 ELSE 0 END,
        o.ScheduledDate ASC,
        o.CreatedAt DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: Update order item status (labor actions: start, complete, decline)
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateOrderItemStatus]
    @OrderItemID INT,
    @ItemStatus NVARCHAR(50),
    @ActualHours INT = NULL,
    @StartTime DATETIME2 = NULL,
    @EndTime DATETIME2 = NULL,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    DECLARE @OrderID INT;
    DECLARE @LaborID INT;
    DECLARE @ScheduledDate DATETIME2;
    DECLARE @RowsAffected INT = 0;

    SELECT
        @OrderID = oi.OrderID,
        @LaborID = oi.LaborID,
        @ScheduledDate = o.ScheduledDate
    FROM [dbo].[OrderItems] oi
    INNER JOIN [dbo].[Orders] o ON oi.OrderID = o.ID
    WHERE oi.ID = @OrderItemID;

    IF @OrderID IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    DECLARE @CurrentItemStatus NVARCHAR(50);
    SELECT @CurrentItemStatus = ItemStatus FROM [dbo].[OrderItems] WHERE ID = @OrderItemID;

    -- Validate work-phase transitions (confirmation is via IVR / app confirm endpoint)
    IF @ItemStatus = N'InProgress' AND @CurrentItemStatus <> N'Assigned'
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    IF @ItemStatus = N'Cancelled' AND @CurrentItemStatus NOT IN (N'Assigned', N'InProgress')
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    IF @ItemStatus = N'Completed' AND @CurrentItemStatus <> N'InProgress'
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    UPDATE [dbo].[OrderItems]
    SET ItemStatus = @ItemStatus,
        ActualHours = COALESCE(@ActualHours, ActualHours),
        StartTime = COALESCE(@StartTime, StartTime),
        EndTime = COALESCE(@EndTime, EndTime)
    WHERE ID = @OrderItemID;

    SET @RowsAffected = @@ROWCOUNT;

    IF @ItemStatus = N'Cancelled'
    BEGIN
        IF @ScheduledDate IS NOT NULL
        BEGIN
            UPDATE la
            SET la.Status = N'Available',
                la.UpdatedAt = GETDATE()
            FROM [dbo].[LaborAvailabilities] la
            WHERE la.LaborID = @LaborID
              AND CAST(la.AvailableDate AS DATE) = CAST(@ScheduledDate AS DATE)
              AND la.Status = N'Booked';
        END
    END

    EXEC [dbo].[sp_RecalculateOrderWorkStatus] @OrderID;

    IF @UpdatedBy IS NOT NULL
    BEGIN
        INSERT INTO [dbo].[OrderTracking] (OrderID, Status, Description, CreatedBy)
        VALUES (
            @OrderID,
            @ItemStatus,
            N'Order item status updated to ' + @ItemStatus,
            @UpdatedBy
        );
    END

    COMMIT TRANSACTION;

    SELECT @RowsAffected AS RowsAffected;
END
GO

-- =============================================
-- Review Management Stored Procedures
-- =============================================

-- SP: Add Labor Review
CREATE PROCEDURE [dbo].[sp_AddLaborReview]
    @OrderItemID INT,
    @EmployerID INT,
    @LaborID INT,
    @Rating INT,
    @Comment NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    -- Check if review already exists
    IF EXISTS (SELECT 1 FROM [dbo].[LaborReviews] WHERE OrderItemID = @OrderItemID)
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Review already exists for this order item', 16, 1);
        RETURN;
    END
    
    -- Add review
    INSERT INTO [dbo].[LaborReviews] (
        OrderItemID, EmployerID, LaborID, Rating, Comment
    )
    VALUES (
        @OrderItemID, @EmployerID, @LaborID, @Rating, @Comment
    );
    
    -- Update labor rating
    DECLARE @AvgRating DECIMAL(3, 2);
    DECLARE @TotalReviews INT;
    
    SELECT 
        @AvgRating = AVG(CAST(Rating AS DECIMAL(3, 2))),
        @TotalReviews = COUNT(*)
    FROM [dbo].[LaborReviews]
    WHERE LaborID = @LaborID AND IsActive = 1;
    
    UPDATE [dbo].[Labors]
    SET Rating = @AvgRating,
        TotalReviews = @TotalReviews
    WHERE ID = @LaborID;
    
    COMMIT TRANSACTION;
    
    SELECT SCOPE_IDENTITY() AS ReviewID;
END
GO

-- =============================================
-- Role and Permission Management
-- =============================================

-- SP: Get Role Permissions
CREATE PROCEDURE [dbo].[sp_GetRolePermissions]
    @RoleID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        rp.ID as PermissionID, rp.FeatureName, rp.CanView, rp.CanCreate,
        rp.CanEdit, rp.CanDelete, rp.IsActive
    FROM [dbo].[RolePermissions] rp
    WHERE rp.RoleID = @RoleID AND rp.IsActive = 1;
END
GO

-- SP: Update Role Permission
CREATE PROCEDURE [dbo].[sp_UpdateRolePermission]
    @RoleID INT,
    @FeatureName NVARCHAR(100),
    @CanView BIT = 0,
    @CanCreate BIT = 0,
    @CanEdit BIT = 0,
    @CanDelete BIT = 0,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM [dbo].[RolePermissions] WHERE RoleID = @RoleID AND FeatureName = @FeatureName)
    BEGIN
        UPDATE [dbo].[RolePermissions]
        SET CanView = @CanView,
            CanCreate = @CanCreate,
            CanEdit = @CanEdit,
            CanDelete = @CanDelete,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @UpdatedBy
        WHERE RoleID = @RoleID AND FeatureName = @FeatureName;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[RolePermissions] (
            RoleID, FeatureName, CanView, CanCreate, CanEdit, CanDelete, CreatedBy
        )
        VALUES (
            @RoleID, @FeatureName, @CanView, @CanCreate, @CanEdit, @CanDelete, @UpdatedBy
        );
    END
END
GO

-- =============================================
-- Additional User Management Stored Procedures
-- =============================================

-- SP: Get User by Mobile Number
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserByMobileNumber]
    @MobileNumber NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT u.*, p.*, r.*
    FROM [dbo].[Users] u
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[Roles] r ON u.RoleID = r.ID
    WHERE u.MobileNumber = @MobileNumber AND u.IsActive = 1;
END
GO

-- SP: Get User by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserById]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT u.*, p.*, r.*
    FROM [dbo].[Users] u
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[Roles] r ON u.RoleID = r.ID
    WHERE u.ID = @UserId AND u.IsActive = 1;
END
GO

-- SP: Create Complete User with Address
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateCompleteUser]
    @MobileNumber NVARCHAR(15),
    @PasswordHash NVARCHAR(255),
    @RoleId INT,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255) = NULL,
    @Street NVARCHAR(255),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Country NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Latitude DECIMAL(10,8) = NULL,
    @Longitude DECIMAL(11,8) = NULL,
    @IsProfileComplete BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @PersonId INT;
    DECLARE @UserId INT;
    
    BEGIN TRY
        -- Create Person record
        INSERT INTO [dbo].[Person] (
            FirstName, LastName, Email, IsActive, CreatedAt
        )
        VALUES (
            @FirstName, @LastName, @Email, 1, GETUTCDATE()
        );
        
        SET @PersonId = SCOPE_IDENTITY();

        -- Create User record
        INSERT INTO [dbo].[Users] (
            PersonID, MobileNumber, RoleID, PasswordHash, 
            IsTemporaryPassword, IsActive, IsProfileComplete, 
            IsMobileVerified, CreatedAt
        )
        VALUES (
            @PersonId, @MobileNumber, @RoleId, @PasswordHash, 
            0, 1, @IsProfileComplete, 1, GETUTCDATE()
        );
        
        SET @UserId = SCOPE_IDENTITY();

        -- Create default address
        INSERT INTO [dbo].[Addresses] (
            UserID, AddressType, Street, City, State, Country, ZipCode,
            Latitude, Longitude, IsDefault, IsActive, CreatedAt
        )
        VALUES (
            @UserId, 'Home', @Street, @City, @State, @Country, @ZipCode,
            @Latitude, @Longitude, 1, 1, GETUTCDATE()
        );

        COMMIT TRANSACTION;
        SELECT @UserId AS UserId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Update Password
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdatePassword]
    @UserId INT,
    @PasswordHash NVARCHAR(255),
    @IsTemporaryPassword BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Users] 
    SET PasswordHash = @PasswordHash, 
        IsTemporaryPassword = @IsTemporaryPassword,
        UpdatedAt = GETUTCDATE()
    WHERE ID = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Update Last Login
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateLastLogin]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Users] 
    SET LastLoginAt = GETUTCDATE() 
    WHERE ID = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Update User
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateUser]
    @Id INT,
    @MobileNumber NVARCHAR(15),
    @RoleId INT,
    @PasswordHash NVARCHAR(255),
    @IsTemporaryPassword BIT,
    @IsActive BIT,
    @IsProfileComplete BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Users] 
    SET MobileNumber = @MobileNumber,
        RoleID = @RoleId,
        PasswordHash = @PasswordHash,
        IsTemporaryPassword = @IsTemporaryPassword,
        IsActive = @IsActive,
        IsProfileComplete = @IsProfileComplete,
        UpdatedAt = GETUTCDATE()
    WHERE ID = @Id;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Check Profile Complete Status
CREATE OR ALTER PROCEDURE [dbo].[sp_IsProfileComplete]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT IsProfileComplete 
    FROM [dbo].[Users] 
    WHERE ID = @UserId;
END
GO

-- SP: Complete User Profile
CREATE OR ALTER PROCEDURE [dbo].[sp_CompleteProfile]
    @UserId INT,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255) = NULL,
    @Street NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL,
    @ZipCode NVARCHAR(20) = NULL,
    @Latitude DECIMAL(10,8) = NULL,
    @Longitude DECIMAL(11,8) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Update Person record
        UPDATE p SET 
            FirstName = @FirstName, 
            LastName = @LastName, 
            Email = @Email,
            UpdatedAt = GETUTCDATE()
        FROM [dbo].[Person] p
        INNER JOIN [dbo].[Users] u ON p.ID = u.PersonID
        WHERE u.ID = @UserId;

        -- Update User record
        UPDATE [dbo].[Users] 
        SET IsProfileComplete = 1, UpdatedAt = GETUTCDATE()
        WHERE ID = @UserId;

        -- Create address if provided
        IF @Street IS NOT NULL
        BEGIN
            INSERT INTO [dbo].[Addresses] (
                UserID, AddressType, Street, City, State, Country, ZipCode,
                Latitude, Longitude, IsDefault, IsActive, CreatedAt
            )
            VALUES (
                @UserId, 'Home', @Street, @City, @State, @Country, @ZipCode,
                @Latitude, @Longitude, 1, 1, GETUTCDATE()
            );
        END

        COMMIT TRANSACTION;
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Get Roles
create or ALTER procedure sp_GetRoles
 @RoleID int=0
 as
 begin
  if(@roleID =0)
  begin
  select ID,RoleName,Description,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy
  from Roles with(nolock)
  where IsActive=1
  order by RoleName
  end
  if(@roleID<>0)
  begin
  select ID,RoleName,Description,IsActive,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy
  from Roles with(nolock) 
  where ID=@RoleID
  and IsActive=1
  order by RoleName
  end
 end 
GO

-- SP: Get Role by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetRoleById]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM [dbo].[Roles] 
    WHERE ID = @RoleId AND IsActive = 1;
END
GO

-- SP: Get Role by Name
CREATE OR ALTER PROCEDURE [dbo].[sp_GetRoleByName]
    @RoleName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM [dbo].[Roles] 
    WHERE RoleName = @RoleName AND IsActive = 1;
END
GO

-- =============================================
-- Cart Management Stored Procedures  
-- =============================================

-- SP: Add to Cart (Authenticated Users)
CREATE OR ALTER PROCEDURE [dbo].[sp_AddToCart]
    @EmployerId INT,
    @LaborId INT,
    @RequiredHours INT,
    @WorkDescription NVARCHAR(1000) = NULL,
    @PreferredDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @DailyRate DECIMAL(10,2);
    DECLARE @TotalAmount DECIMAL(10,2);
    DECLARE @CartId INT;
    DECLARE @PreferredDateOnly DATE;
    
    BEGIN TRY
        -- Get labor hourly rate
        SELECT @DailyRate = DailyRate 
        FROM [dbo].[Labors] 
        WHERE ID = @LaborId AND IsActive = 1;
        
        IF @DailyRate IS NULL
        BEGIN
            RAISERROR('Labor not found or inactive', 16, 1);
        END

        IF @PreferredDate IS NULL
        BEGIN
            RAISERROR('Please select preferred date.', 16, 1);
        END

        SET @PreferredDateOnly = CAST(@PreferredDate AS DATE);

        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[LaborAvailabilities]
            WHERE LaborID = @LaborId
              AND AvailableDate = @PreferredDateOnly
              AND Status = N'Available'
        )
        BEGIN
            RAISERROR('Labor is not available for this date. Please select another date.', 16, 1);
        END
        
        SET @TotalAmount = @DailyRate * @RequiredHours;

        INSERT INTO [dbo].[Carts] (
            EmployerID, LaborID, RequiredHours, DailyRate, TotalAmount, 
            WorkDescription, PreferredDate, CreatedAt
        )
        VALUES (
            @EmployerId, @LaborId, @RequiredHours, @Hour    lyRate, @TotalAmount,
            @WorkDescription, @PreferredDate, GETUTCDATE()
        );
        
        SET @CartId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        SELECT @CartId AS CartId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Add to Session Cart (Anonymous Users)
CREATE OR ALTER PROCEDURE [dbo].[sp_AddToSessionCart]
    @SessionId NVARCHAR(100),
    @LaborId INT,
    @RequiredHours INT,
    @WorkDescription NVARCHAR(1000) = NULL,
    @PreferredDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    DECLARE @DailyRate DECIMAL(10,2);
    DECLARE @TotalAmount DECIMAL(10,2);
    DECLARE @CartId INT;
    DECLARE @PreferredDateOnly DATE;
    
    BEGIN TRY
        -- Ensure SessionCarts table exists
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SessionCarts')
        BEGIN
            CREATE TABLE [dbo].[SessionCarts] (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                SessionID NVARCHAR(100) NOT NULL,
                LaborID INT NOT NULL,
                RequiredHours INT NOT NULL,
                DailyRate DECIMAL(10,2) NOT NULL,
                TotalAmount DECIMAL(10,2) NOT NULL,
                WorkDescription NVARCHAR(1000),
                PreferredDate DATETIME2,
                CreatedAt DATETIME2 DEFAULT GETUTCDATE()
            );
        END

        -- Get labor hourly rate
        SELECT @DailyRate = DailyRate 
        FROM [dbo].[Labors] 
        WHERE ID = @LaborId AND IsActive = 1;
        
        IF @DailyRate IS NULL
        BEGIN
            RAISERROR('Labor not found or inactive', 16, 1);
        END

        IF @PreferredDate IS NULL
        BEGIN
            RAISERROR('Please select preferred date.', 16, 1);
        END

        SET @PreferredDateOnly = CAST(@PreferredDate AS DATE);

        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[LaborAvailabilities]
            WHERE LaborID = @LaborId
              AND AvailableDate = @PreferredDateOnly
              AND Status = N'Available'
        )
        BEGIN
            RAISERROR('Labor is not available for this date. Please select another date.', 16, 1);
        END
        
        SET @TotalAmount = @DailyRate * @RequiredHours;

        INSERT INTO [dbo].[SessionCarts] (
            SessionID, LaborID, RequiredHours, DailyRate, TotalAmount, 
            WorkDescription, PreferredDate, CreatedAt
        )
        VALUES (
            @SessionId, @LaborId, @RequiredHours, @DailyRate, @TotalAmount,
            @WorkDescription, @PreferredDate, GETUTCDATE()
        );
        
        SET @CartId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        SELECT @CartId AS CartId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Get Cart Items (Authenticated)
CREATE OR ALTER PROCEDURE [dbo].[sp_GetCartItems]
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ID as CartId, c.LaborID as LaborId, c.RequiredHours, c.DailyRate, 
        c.TotalAmount, c.WorkDescription, c.PreferredDate, c.CreatedAt,
        CONCAT(p.FirstName, ' ', p.LastName) as LaborName,
        p.ProfilePicture, lt.TypeName as LaborType, l.Specialization,
        l.Rating, l.TotalReviews,
        CASE
            WHEN c.PreferredDate IS NULL THEN CAST(1 AS BIT)
            WHEN CAST(c.PreferredDate AS DATE) < CAST(GETDATE() AS DATE) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS IsExpired,
        CASE
            WHEN c.PreferredDate IS NULL THEN CAST(1 AS BIT)
            WHEN NOT EXISTS (
                SELECT 1
                FROM [dbo].[LaborAvailabilities] la
                WHERE la.LaborID = c.LaborID
                  AND CAST(la.AvailableDate AS DATE) = CAST(c.PreferredDate AS DATE)
                  AND la.Status = N'Available'
            ) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS IsUnavailableNow
    FROM [dbo].[Carts] c
    INNER JOIN [dbo].[Labors] l ON c.LaborID = l.ID
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    WHERE c.EmployerID = @EmployerId
    ORDER BY c.CreatedAt DESC;
END
GO

-- SP: Get Session Cart Items
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSessionCartItems]
    @SessionId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sc.ID as CartId, sc.LaborID as LaborId, sc.RequiredHours, sc.DailyRate, 
        sc.TotalAmount, sc.WorkDescription, sc.PreferredDate, sc.CreatedAt,
        CONCAT(p.FirstName, ' ', p.LastName) as LaborName,
        p.ProfilePicture, lt.TypeName as LaborType, l.Specialization,
        l.Rating, l.TotalReviews,
        CASE
            WHEN sc.PreferredDate IS NULL THEN CAST(1 AS BIT)
            WHEN CAST(sc.PreferredDate AS DATE) < CAST(GETDATE() AS DATE) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS IsExpired,
        CASE
            WHEN sc.PreferredDate IS NULL THEN CAST(1 AS BIT)
            WHEN NOT EXISTS (
                SELECT 1
                FROM [dbo].[LaborAvailabilities] la
                WHERE la.LaborID = sc.LaborID
                  AND CAST(la.AvailableDate AS DATE) = CAST(sc.PreferredDate AS DATE)
                  AND la.Status = N'Available'
            ) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS IsUnavailableNow
    FROM [dbo].[SessionCarts] sc
    INNER JOIN [dbo].[Labors] l ON sc.LaborID = l.ID
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    WHERE sc.SessionID = @SessionId
    ORDER BY sc.CreatedAt DESC;
END
GO

-- SP: Get Cart Item Count (Authenticated)
CREATE OR ALTER PROCEDURE [dbo].[sp_GetCartItemCount]
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS ItemCount
    FROM [dbo].[Carts] 
    WHERE EmployerID = @EmployerId;
END
GO

-- SP: Get Session Cart Item Count
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSessionCartItemCount]
    @SessionId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS ItemCount
    FROM [dbo].[SessionCarts] 
    WHERE SessionID = @SessionId;
END
GO

-- SP: Merge Session Cart
CREATE OR ALTER PROCEDURE [dbo].[sp_MergeSessionCart]
    @SessionId NVARCHAR(100),
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Check if session cart exists and has items
        IF EXISTS (SELECT 1 FROM [dbo].[SessionCarts] WHERE SessionID = @SessionId)
        BEGIN
            -- Move session cart items to user cart
            INSERT INTO [dbo].[Carts] (
                EmployerID, LaborID, RequiredHours, DailyRate, TotalAmount,
                WorkDescription, PreferredDate, CreatedAt
            )
            SELECT 
                @UserId, LaborID, RequiredHours, DailyRate, TotalAmount,
                WorkDescription, PreferredDate, GETUTCDATE()
            FROM [dbo].[SessionCarts]
            WHERE SessionID = @SessionId;
            
            -- Delete session cart items
            DELETE FROM [dbo].[SessionCarts] WHERE SessionID = @SessionId;
        END
        
        COMMIT TRANSACTION;
        SELECT 1 AS Success;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Update Cart Item
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateCartItem]
    @CartId INT,
    @EmployerId INT,
    @RequiredHours INT,
    @WorkDescription NVARCHAR(1000) = NULL,
    @PreferredDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    if not exists(select top 1 1 from LaborAvailabilities where LaborID=@laborId and cast(AvailableDate as date)=cast(@PreferredDate as date))
	begin 
	 raiserror('Labor is not available on given date',16,1)
	 return;
	end
    
    UPDATE [dbo].[Carts] 
    SET RequiredHours = @RequiredHours,
        TotalAmount = (SELECT DailyRate FROM [dbo].[Labors] WHERE ID = [dbo].[Carts].LaborID) * (@RequiredHours/8),
        WorkDescription = @WorkDescription,
        PreferredDate = @PreferredDate
    WHERE ID = @CartId AND EmployerID = @EmployerId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Remove from Cart
CREATE OR ALTER PROCEDURE [dbo].[sp_RemoveFromCart]
    @CartId INT,
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[Carts] 
    WHERE ID = @CartId AND EmployerID = @EmployerId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Clear Cart
CREATE OR ALTER PROCEDURE [dbo].[sp_ClearCart]
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[Carts] 
    WHERE EmployerID = @EmployerId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Labor Management Stored Procedures
-- =============================================

-- SP: Search Labors
CREATE OR ALTER PROCEDURE [dbo].[sp_SearchLabors]
    @LaborTypeId INT = NULL,
    @SearchText NVARCHAR(255) = NULL,
    @AvailabilityStatus NVARCHAR(50) = NULL,
    @MinRating DECIMAL(3,2) = 0,
    @MaxDailyRate DECIMAL(10,2) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        l.ID as LaborId, l.UserID as UserId, p.FirstName, p.LastName, p.ProfilePicture,
        l.LaborTypeID, lt.TypeName as LaborType, l.Specialization, l.ExperienceYears,
        l.Rating, l.TotalReviews, l.DailyRate, l.MinimumHours, l.MaximumHours,
        l.AvailabilityStatus, l.IsVerified, a.City, a.State
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    LEFT JOIN [dbo].[Addresses] a ON u.ID = a.UserID AND a.IsDefault = 1
    WHERE l.IsActive = 1 
        AND u.IsActive = 1
        AND (@LaborTypeId IS NULL OR l.LaborTypeID = @LaborTypeId)
        AND (@SearchText IS NULL OR (p.FirstName LIKE '%' + @SearchText + '%' OR p.LastName LIKE '%' + @SearchText + '%' OR l.Specialization LIKE '%' + @SearchText + '%'))
        AND (@AvailabilityStatus IS NULL OR l.AvailabilityStatus = @AvailabilityStatus)
        AND (l.Rating >= @MinRating)
        AND (@MaxDailyRate IS NULL OR l.DailyRate <= @MaxDailyRate)
    ORDER BY l.Rating DESC, l.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: Get Labor Details
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborDetails]
    @LaborId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get labor info
    SELECT 
        l.ID as LaborId, l.UserID as UserId, p.FirstName, p.LastName, p.ProfilePicture,
        l.LaborTypeID, lt.TypeName as LaborType, l.Specialization, l.ExperienceYears,
        l.Rating, l.TotalReviews, l.DailyRate, l.MinimumHours, l.MaximumHours,
        l.AvailabilityStatus, l.IsVerified, a.City, a.State
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    LEFT JOIN [dbo].[Addresses] a ON u.ID = a.UserID AND a.IsDefault = 1
    WHERE l.ID = @LaborId AND l.IsActive = 1;
    
    -- Get skills
    SELECT ID as SkillId, SkillName, ProficiencyLevel
    FROM [dbo].[LaborSkills]
    WHERE LaborID = @LaborId
    ORDER BY SkillName;
    
    -- Get recent reviews
    SELECT TOP 5 
        lr.ID as ReviewId, lr.Rating, lr.Comment, lr.CreatedAt,
        CONCAT(p.FirstName, ' ', p.LastName) as EmployerName
    FROM [dbo].[LaborReviews] lr
    INNER JOIN [dbo].[Users] u ON lr.EmployerID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE lr.LaborID = @LaborId AND lr.IsActive = 1
    ORDER BY lr.CreatedAt DESC;
END
GO

-- SP: Get Labor Types
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborTypes]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM [dbo].[LaborTypes] 
    WHERE IsActive = 1 
    ORDER BY TypeName;
END
GO

-- SP: Create Labor Profile
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateLaborProfile]
    @UserId INT,
    @LaborTypeId INT,
    @Specialization NVARCHAR(255),
    @ExperienceYears INT,
    @DailyRate DECIMAL(10,2),
    @MinimumHours INT,
    @MaximumHours INT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[Labors] (
        UserID, LaborTypeID, Specialization, ExperienceYears, 
        DailyRate, MinimumHours, MaximumHours, AvailabilityStatus,
        IsActive, CreatedAt
    )
    VALUES (
        @UserId, @LaborTypeId, @Specialization, @ExperienceYears,
        @DailyRate, @MinimumHours, @MaximumHours, 'Available',
        1, GETUTCDATE()
    );
    
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LaborId;
END
GO

-- SP: Update Labor Profile
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateLaborProfile]
    @Id INT,
    @LaborTypeId INT,
    @Specialization NVARCHAR(255),
    @ExperienceYears INT,
    @DailyRate DECIMAL(10,2),
    @MinimumHours INT,
    @MaximumHours INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Labors] 
    SET LaborTypeID = @LaborTypeId, 
        Specialization = @Specialization,
        ExperienceYears = @ExperienceYears, 
        DailyRate = @DailyRate,
        MinimumHours = @MinimumHours, 
        MaximumHours = @MaximumHours,
        UpdatedAt = GETUTCDATE()
    WHERE ID = @Id;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Update Labor Availability
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateLaborAvailability]
    @LaborId INT,
    @AvailabilityStatus NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Labors] 
    SET AvailabilityStatus = @AvailabilityStatus, 
        UpdatedAt = GETUTCDATE()
    WHERE ID = @LaborId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Get Labor Skills
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborSkills]
    @LaborId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM [dbo].[LaborSkills] 
    WHERE LaborID = @LaborId 
    ORDER BY SkillName;
END
GO

-- SP: Add Labor Skill
CREATE OR ALTER PROCEDURE [dbo].[sp_AddLaborSkill]
    @LaborId INT,
    @SkillName NVARCHAR(100),
    @ProficiencyLevel NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[LaborSkills] (
        LaborID, SkillName, ProficiencyLevel, CreatedAt
    )
    VALUES (
        @LaborId, @SkillName, @ProficiencyLevel, GETUTCDATE()
    );
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Remove Labor Skill
CREATE OR ALTER PROCEDURE [dbo].[sp_RemoveLaborSkill]
    @SkillId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[LaborSkills] 
    WHERE ID = @SkillId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Address Management Stored Procedures
-- =============================================

-- SP: Get User Addresses
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserAddresses]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM [dbo].[Addresses]
    WHERE UserID = @UserId AND IsActive = 1
    ORDER BY IsDefault DESC, CreatedAt DESC;
END
GO

-- SP: Get Address by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAddressById]
    @AddressId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM [dbo].[Addresses]
    WHERE ID = @AddressId AND UserID = @UserId AND IsActive = 1;
END
GO

-- SP: Create Address
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateAddress]
    @UserId INT,
    @AddressType NVARCHAR(50),
    @Street NVARCHAR(255),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Country NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Latitude DECIMAL(10,8) = NULL,
    @Longitude DECIMAL(11,8) = NULL,
    @IsDefault BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- If this is set as default, unset other defaults
        IF @IsDefault = 1
        BEGIN
            UPDATE [dbo].[Addresses] 
            SET IsDefault = 0 
            WHERE UserID = @UserId;
        END

        INSERT INTO [dbo].[Addresses] (
            UserID, AddressType, Street, City, State, Country, ZipCode,
            Latitude, Longitude, IsDefault, IsActive, CreatedAt
        )
        VALUES (
            @UserId, @AddressType, @Street, @City, @State, @Country, @ZipCode,
            @Latitude, @Longitude, @IsDefault, 1, GETUTCDATE()
        );
        
        COMMIT TRANSACTION;
        SELECT CAST(SCOPE_IDENTITY() AS INT) AS AddressId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Update Address
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateAddress]
    @AddressId INT,
    @UserId INT,
    @AddressType NVARCHAR(50),
    @Street NVARCHAR(255),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Country NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Latitude DECIMAL(10,8) = NULL,
    @Longitude DECIMAL(11,8) = NULL,
    @IsDefault BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- If this is set as default, unset other defaults
        IF @IsDefault = 1
        BEGIN
            UPDATE [dbo].[Addresses] 
            SET IsDefault = 0 
            WHERE UserID = @UserId AND ID != @AddressId;
        END

        UPDATE [dbo].[Addresses]
        SET AddressType = @AddressType,
            Street = @Street,
            City = @City,
            State = @State,
            Country = @Country,
            ZipCode = @ZipCode,
            Latitude = @Latitude,
            Longitude = @Longitude,
            IsDefault = @IsDefault,
            UpdatedAt = GETUTCDATE()
        WHERE ID = @AddressId AND UserID = @UserId;
        
        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS RowsAffected;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: Delete Address
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteAddress]
    @AddressId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Addresses] 
    SET IsActive = 0, UpdatedAt = GETUTCDATE()
    WHERE ID = @AddressId AND UserID = @UserId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Get Default Address
CREATE OR ALTER PROCEDURE [dbo].[sp_GetDefaultAddress]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1 * FROM [dbo].[Addresses]
    WHERE UserID = @UserId AND IsActive = 1 AND IsDefault = 1;
END
GO

-- =============================================
-- Review Management Stored Procedures
-- =============================================

-- SP: Add Labor Review
CREATE OR ALTER PROCEDURE [dbo].[sp_AddLaborReview]
    @OrderItemId INT,
    @EmployerId INT,
    @LaborId INT,
    @Rating INT,
    @Comment NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO [dbo].[LaborReviews] (
        OrderItemID, EmployerID, LaborID, Rating, Comment, IsActive, CreatedAt
    )
    VALUES (
        @OrderItemId, @EmployerId, @LaborId, @Rating, @Comment, 1, GETUTCDATE()
    );
    
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReviewId;
END
GO

-- SP: Get Labor Reviews
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborReviews]
    @LaborId INT,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    SELECT 
        lr.ID as ReviewId, lr.Rating, lr.Comment, lr.CreatedAt,
        CONCAT(p.FirstName, ' ', p.LastName) as EmployerName
    FROM [dbo].[LaborReviews] lr
    INNER JOIN [dbo].[Users] u ON lr.EmployerID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE lr.LaborID = @LaborId AND lr.IsActive = 1
    ORDER BY lr.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- SP: Get Review by ID
CREATE OR ALTER PROCEDURE [dbo].[sp_GetReviewById]
    @ReviewId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lr.ID as ReviewId, lr.OrderItemID as OrderItemId, lr.Rating, lr.Comment, lr.CreatedAt,
        CONCAT(p.FirstName, ' ', p.LastName) as EmployerName
    FROM [dbo].[LaborReviews] lr
    INNER JOIN [dbo].[Users] u ON lr.EmployerID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE lr.ID = @ReviewId AND lr.IsActive = 1;
END
GO

-- SP: Update Review
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateReview]
    @ReviewId INT,
    @EmployerId INT,
    @Rating INT,
    @Comment NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[LaborReviews]
    SET Rating = @Rating, 
        Comment = @Comment, 
        UpdatedAt = GETUTCDATE()
    WHERE ID = @ReviewId AND EmployerID = @EmployerId AND IsActive = 1;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Delete Review
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteReview]
    @ReviewId INT,
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[LaborReviews]
    SET IsActive = 0, UpdatedAt = GETUTCDATE()
    WHERE ID = @ReviewId AND EmployerID = @EmployerId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- SP: Check if User Reviewed Order Item
CREATE OR ALTER PROCEDURE [dbo].[sp_HasUserReviewedOrderItem]
    @OrderItemId INT,
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(*) AS ReviewCount
    FROM [dbo].[LaborReviews] 
    WHERE OrderItemID = @OrderItemId AND EmployerID = @EmployerId AND IsActive = 1;
END
GO

-- SP: Get Labor Average Rating
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLaborAverageRating]
    @LaborId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COALESCE(AVG(CAST(Rating AS DECIMAL(3,2))), 0.00) AS AverageRating
    FROM [dbo].[LaborReviews]
    WHERE LaborID = @LaborId AND IsActive = 1;
END
GO 

--Create new role stored procedure
CREATE PROCEDURE [DBO].[sp_CreateRole]
  @RoleName varchar(100),
  @Description varchar(255)=null,
  @CreatedBy int=null
As
 begin
  set nocount on;

   if exists(select top 1 1 from Roles where RoleName=@RoleName)
   begin
    Raiserror('Role name already exists',16,1);
	return
   end
   else
   begin 
     insert into Roles(RoleName,Description,IsActive,CreatedAt,CreatedBy)
	 values(@RoleName,@Description,1,GETDATE(),@CreatedBy)
	 select cast(SCOPE_IDENTITY() as int) as NewRoleId
   end
  end
GO
  --Update role stored procedure
  create procedure [dbo].[sp_UpdateRole]
 @RoleId int,
 @RoleName varchar(100),
 @Description varchar(250)=null,
 @IsActive bit =1,
 @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE RoleName = @RoleName AND ID <> @RoleId)
    BEGIN
        RAISERROR('Another role already uses this name.', 16, 1);
        RETURN;
    END
	else
	begin
    UPDATE [dbo].[Roles]
    SET RoleName = @RoleName,
        Description = @Description,
        IsActive = @IsActive,
        UpdatedAt = getdate(),
        UpdatedBy = @UpdatedBy
    WHERE ID = @RoleId;
    SELECT @@ROWCOUNT AS RowsAffected;
	end
END
GO
--Delete role stored procedure
CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteRole]
    @RoleId INT,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE RoleID = @RoleId AND IsActive = 1)
    BEGIN
        RAISERROR('Cannot delete role: users are still assigned to it.', 16, 1);
        RETURN;
    END
	else
	begin
    UPDATE [dbo].[Roles]
    SET IsActive = 0,
        UpdatedAt = getdate(),
        UpdatedBy = @UpdatedBy
    WHERE ID = @RoleId;
    SELECT @@ROWCOUNT AS RowsAffected;
	end
END
GO
--create role permission stored procedure
CREATE PROCEDURE [dbo].[sp_CreateRolePermission]
    @RoleID INT,
    @FeatureName NVARCHAR(100),
    @CanView BIT = 0,
    @CanCreate BIT = 0,
    @CanEdit BIT = 0,
    @CanDelete BIT = 0,
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE ID = @RoleID AND IsActive = 1)
    BEGIN
        RAISERROR('Role does not exist or is inactive.', 16, 1);
        RETURN;
    END
    IF EXISTS (
        SELECT 1 FROM [dbo].[RolePermissions]
        WHERE RoleID = @RoleID AND FeatureName = @FeatureName AND IsActive = 1)
    BEGIN
        RAISERROR('Permission for this feature already exists for the role.', 16, 1);
        RETURN;
    END
    INSERT INTO [dbo].[RolePermissions] (
        RoleID, FeatureName, CanView, CanCreate, CanEdit, CanDelete, IsActive, CreatedAt, CreatedBy)
    VALUES (
        @RoleID, @FeatureName, @CanView, @CanCreate, @CanEdit, @CanDelete, 1, GETUTCDATE(), @CreatedBy);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewPermissionId;
END
--get  permissions stored procedure
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPermissionById]
    @PermissionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM [dbo].[RolePermissions]
    WHERE ID = @PermissionId;
END
GO
--get role permissions stored procedure

ALTER PROCEDURE [dbo].[sp_GetRolePermissionById]
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
	if not exists(select top 1 1 from Roles with(nolock) where ID=@RoleId)
	begin
	 raiserror('Role does not exist',16,1)
	 return
	end
	else
	begin
   select rp.ID,r.ID,r.RoleName,rp.FeatureName,rp.CanView,rp.CanCreate,rp.CanEdit,rp.CanEdit,rp.CanDelete,
     rp.IsActive
   from Roles r with(nolock),RolePermissions rp with(nolock)
   where r.ID=rp.RoleID
   and r.ID=@RoleId
   and rp.IsActive=1
   end
END
GO
--update role permission stored procedure
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateRolePermissionById]
    @PermissionId INT,
    @FeatureName NVARCHAR(100),
    @CanView BIT = 0,
    @CanCreate BIT = 0,
    @CanEdit BIT = 0,
    @CanDelete BIT = 0,
    @IsActive BIT = 1,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
	if not exists(select top 1 1 from RolePermissions where ID=@PermissionId and IsActive=1)
	begin
		Raiserror('Rolepermission does not exists or inactive.',16,1)
		return;
	end
	else
	begin
    UPDATE [dbo].[RolePermissions]
    SET FeatureName = @FeatureName,
        CanView = @CanView,
        CanCreate = @CanCreate,
        CanEdit = @CanEdit,
        CanDelete = @CanDelete,
        IsActive = @IsActive,
        UpdatedAt = getdate(),
        UpdatedBy = @UpdatedBy
    WHERE ID = @PermissionId and IsActive=1
    SELECT @@ROWCOUNT AS RowsAffected;
	end
END
GO
--Delete role permission stored procedure
CREATE or ALTER PROCEDURE [dbo].[sp_DeleteRolePermission]
    @PermissionId INT,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
	if not exists(select top 1 1 from RolePermissions where ID=@PermissionId and IsActive=1)
	begin
		Raiserror('Rolepermission does not exists or inactive.',16,1)
		return;
	end
	else
	begin 
    UPDATE [dbo].[RolePermissions]
    SET IsActive = 0,
        UpdatedAt = getdate(),
        UpdatedBy = @UpdatedBy
    WHERE ID = @PermissionId and IsActive=1
    SELECT @@ROWCOUNT AS RowsAffected;
	end
END
--get all users stored procedure
create or ALTER   PROCEDURE [dbo].[sp_AdminGetUsers]
    @Role NVARCHAR(100) = NULL,
	@InactiveUsers bit=0,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    SELECT
        u.ID AS UserId,
		u.UserName,
		p.Email,
        u.MobileNumber,
        r.RoleName,
        p.FirstName,
        p.LastName,
        LTRIM(RTRIM(ISNULL(p.FirstName, N'') + N' ' + ISNULL(p.LastName, N''))) AS DisplayName,
        u.IsActive,
        u.IsProfileComplete,
        u.CreatedAt
    FROM [dbo].[Users] u
    INNER JOIN [dbo].[Roles] r ON u.RoleID = r.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE (@Role IS NULL OR r.RoleName = @Role)
	 AND (@InactiveUsers = 1 OR u.IsActive = 1)
    ORDER BY u.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
-- set user active status stored procedure
CREATE OR ALTER PROCEDURE [dbo].[sp_AdminSetUserActive]
    @UserId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Users]
    SET IsActive = @IsActive, UpdatedAt = getdate()
    WHERE ID = @UserId;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
--verify labor profile stored procedure
CREATE OR ALTER PROCEDURE [dbo].[sp_AdminVerifyLabor]
    @LaborId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Labors]
    SET IsVerified = 1, UpdatedAt = getdate()
    WHERE ID = @LaborId AND IsActive = 1;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
--onboard labor stored procedure
Create   PROCEDURE [dbo].[sp_AdminOnboardLabor]
    @MobileNumber NVARCHAR(15),
	@UserName varchar(100)=null,
    @PasswordHash NVARCHAR(255),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255) = NULL,
    @LaborTypeId INT,
    @DailyRate DECIMAL(10,2),
    @Specialization NVARCHAR(255) = NULL,
    @ExperienceYears INT = 0,
    @CreatedBy INT = NULL,
	@Street NVARCHAR(255),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Country NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Latitude DECIMAL(10, 8) = NULL,
    @Longitude DECIMAL(11, 8) = NULL,
	@ProfilePicture nvarchar(500)=null
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE MobileNumber = @MobileNumber)
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Mobile number already registered.', 16, 1);
            RETURN;
        END
        DECLARE @LaborRoleId INT;
        SELECT @LaborRoleId = ID FROM [dbo].[Roles] WHERE RoleName = N'Labor' AND IsActive = 1;
        IF @LaborRoleId IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Labor role not found.', 16, 1);
            RETURN;
        END
if not exists (select top 1 1 from Users u, Roles r, RolePermissions rp
	  where u.RoleID=r.ID
	  and r.ID=rp.RoleID
	  and rp.CanCreate=1)
	  begin 
	    raiserror('User does not have permission to create labor.',16,1)
		return
	  end

        DECLARE @PersonId INT;
        INSERT INTO [dbo].[Person] (FirstName, LastName, Email,ProfilePicture, IsActive, CreatedAt)
        VALUES (@FirstName, @LastName, @Email,@ProfilePicture, 1, Getdate());
        SET @PersonId = SCOPE_IDENTITY();
        DECLARE @UserId INT;
        INSERT INTO [dbo].[Users] (
            PersonID, MobileNumber,UserName, RoleID, PasswordHash,
            IsTemporaryPassword, IsActive, IsProfileComplete, IsMobileVerified, CreatedAt, CreatedBy
        )
        VALUES (
            @PersonId, @MobileNumber,@UserName, @LaborRoleId, @PasswordHash,
            1, 1, 1, 1, Getdate(), @CreatedBy
        );
        SET @UserId = SCOPE_IDENTITY();
        INSERT INTO [dbo].[Labors] (
            UserID, LaborTypeID, Specialization, ExperienceYears,
            DailyRate, MinimumHours, MaximumHours, AvailabilityStatus, IsActive, CreatedAt, CreatedBy
        )
        VALUES (
            @UserId, @LaborTypeId, @Specialization, @ExperienceYears,
            @DailyRate, 1, 24, N'Available', 1, Getdate(), @CreatedBy
        );
        DECLARE @LaborId INT = SCOPE_IDENTITY();

		insert into Addresses(UserID,AddressType,Street,City,State,Country,ZipCode,Latitude,Longitude,IsDefault,IsActive,CreatedAt)
		 VALUES (
            @UserId, N'Home', @Street, @City, @State, @Country, @ZipCode,
            @Latitude, @Longitude, 1, 1, getdate()
        )
        COMMIT TRANSACTION;
        SELECT @UserId AS UserId, @LaborId AS LaborId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
CREATE OR ALTER PROCEDURE [dbo].[sp_AdminGetAllOrders]
    @OrderStatus NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    SELECT
        o.ID AS OrderId,
        o.OrderNumber,
        o.EmployerID AS EmployerId,
        LTRIM(RTRIM(ISNULL(p.FirstName, N'') + N' ' + ISNULL(p.LastName, N''))) AS EmployerName,
        u.MobileNumber AS EmployerMobile,
        o.TotalAmount,
        o.OrderStatus,
        o.PaymentStatus,
        o.ScheduledDate,
        o.CreatedAt
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[Users] u ON o.EmployerID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE (@OrderStatus IS NULL OR o.OrderStatus = @OrderStatus)
    ORDER BY o.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_AdminGetAllLabors]
    @VerifiedOnly BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    SELECT
        l.ID AS LaborId,
        l.UserID AS UserId,
        u.MobileNumber,
        LTRIM(RTRIM(ISNULL(p.FirstName, N'') + N' ' + ISNULL(p.LastName, N''))) AS LaborName,
        lt.TypeName AS LaborType,
        l.Specialization,
        l.DailyRate,
        l.Rating,
        l.IsVerified,
        l.IsActive,
        l.AvailabilityStatus
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    WHERE (@VerifiedOnly IS NULL OR l.IsVerified = @VerifiedOnly)
    ORDER BY l.CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
--set user active status stored procedure
Create   PROCEDURE [dbo].[sp_AdminSetLaborActive]
    @LaborId INT,
    @IsActive BIT,
	@UpdatedBy int=0
AS
BEGIN
    SET NOCOUNT ON;
     update Labors with(nolock)
	 set IsActive=@IsActive,UpdatedAt=GETDATE(),UpdatedBy=@UpdatedBy
	 where ID=@LaborId
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
-- =============================================
-- Admin: get one labor with user/person/address for edit form
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_AdminGetLaborForEdit]
    @LaborId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.ID AS LaborId,
        l.UserID AS UserId,
        u.MobileNumber,
        u.UserName,
        p.FirstName,
        p.LastName,
        p.Email,
        p.ProfilePicture,
        l.LaborTypeID AS LaborTypeId,
        lt.TypeName AS LaborTypeName,
        l.Specialization,
        l.ExperienceYears,
        l.DailyRate,
        l.MinimumHours,
        l.MaximumHours,
        l.AvailabilityStatus,
        l.IsVerified,
        l.IsActive AS LaborListingActive,
        ISNULL(a.ID, 0) AS AddressId,
        a.Street,
        a.City,
        a.State,
        a.Country,
        a.ZipCode,
        a.Latitude,
        a.Longitude
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    INNER JOIN [dbo].[Roles] r ON u.RoleID = r.ID AND r.RoleName = N'Labor' AND r.IsActive = 1
    LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
    LEFT JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    LEFT JOIN [dbo].[Addresses] a ON a.UserID = u.ID AND a.IsDefault = 1 AND a.IsActive = 1
    WHERE l.ID = @LaborId;
END
GO

-- =============================================
-- Admin: full update labor + linked user/person/default address
-- Password: pass @PasswordHash NULL to leave unchanged
-- =============================================
CREATE or ALTER PROCEDURE [dbo].[sp_AdminUpdateLaborFull]
    @LaborId INT,
    @MobileNumber NVARCHAR(15),
    @UserName NVARCHAR(100) = NULL,
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255) = NULL,
    @ProfilePicture NVARCHAR(500) = NULL,
    @PasswordHash NVARCHAR(255) = NULL,
    @Street NVARCHAR(255),
    @City NVARCHAR(100),
    @State NVARCHAR(100),
    @Country NVARCHAR(100),
    @ZipCode NVARCHAR(20),
    @Latitude DECIMAL(10, 8) = NULL,
    @Longitude DECIMAL(11, 8) = NULL,
    @LaborTypeId INT,
    @Specialization NVARCHAR(255) = NULL,
    @ExperienceYears INT = 0,
    @DailyRate DECIMAL(10, 2),
    @MinimumHours INT = 1,
    @MaximumHours INT = 24,
    @AvailabilityStatus NVARCHAR(50) = N'Available',
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UserId INT;
    SELECT @UserId = l.UserID
    FROM [dbo].[Labors] l
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    INNER JOIN [dbo].[Roles] r ON u.RoleID = r.ID AND r.RoleName = N'Labor'
    WHERE l.ID = @LaborId;
    IF @UserId IS NULL
    BEGIN
        RAISERROR('Labor not found or not a labor user.', 16, 1);
        RETURN;
    END
    IF EXISTS (
        SELECT 1 FROM [dbo].[Users]
        WHERE MobileNumber = @MobileNumber AND ID <> @UserId
    )
    BEGIN
        RAISERROR('Mobile number already registered.', 16, 1);
        RETURN;
    END

	if not exists (select top 1 1 from Users u, Roles r, RolePermissions rp
	  where u.RoleID=r.ID
	  and r.ID=rp.RoleID
	  and rp.CanEdit=1)
	  begin 
	    raiserror('User does not have permission to edit.',16,1)
		return
	  end

    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE p
        SET
            FirstName = @FirstName,
            LastName = @LastName,
            Email = @Email,
            ProfilePicture = @ProfilePicture,
            UpdatedAt = GETUTCDATE()
        FROM [dbo].[Person] p
        INNER JOIN [dbo].[Users] u ON p.ID = u.PersonID
        WHERE u.ID = @UserId;
        UPDATE [dbo].[Users]
        SET
            MobileNumber = @MobileNumber,
            UserName = @UserName,
            PasswordHash = CASE WHEN @PasswordHash IS NOT NULL THEN @PasswordHash ELSE PasswordHash END,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @UpdatedBy
        WHERE ID = @UserId;
        IF EXISTS (
            SELECT 1 FROM [dbo].[Addresses]
            WHERE UserID = @UserId AND IsDefault = 1
        )
        BEGIN
            UPDATE [dbo].[Addresses]
            SET
                Street = @Street,
                City = @City,
                State = @State,
                Country = @Country,
                ZipCode = @ZipCode,
                Latitude = @Latitude,
                Longitude = @Longitude,
                UpdatedAt = GETUTCDATE()
            WHERE UserID = @UserId AND IsDefault = 1;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[Addresses] (
                UserID, AddressType, Street, City, State, Country, ZipCode,
                Latitude, Longitude, IsDefault, IsActive, CreatedAt
            )
            VALUES (
                @UserId, N'Home', @Street, @City, @State, @Country, @ZipCode,
                @Latitude, @Longitude, 1, 1, GETUTCDATE()
            );
        END
        UPDATE [dbo].[Labors]
        SET
            LaborTypeID = @LaborTypeId,
            Specialization = @Specialization,
            ExperienceYears = @ExperienceYears,
            DailyRate = @DailyRate,
            MinimumHours = @MinimumHours,
            MaximumHours = @MaximumHours,
            AvailabilityStatus = @AvailabilityStatus,
            UpdatedAt = GETUTCDATE(),
            UpdatedBy = @UpdatedBy
        WHERE ID = @LaborId;
        COMMIT TRANSACTION;
        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
--create update role permission stored procedure (upsert per RoleID + FeatureName)
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateUpdateRolePermission]
    @RoleID INT,
    @FeatureName NVARCHAR(100),
    @CanView BIT = 0,
    @CanCreate BIT = 0,
    @CanEdit BIT = 0,
    @CanDelete BIT = 0,
    @CreatedBy INT = NULL,
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE ID = @RoleID AND IsActive = 1)
    BEGIN
        RAISERROR('Role does not exist or is inactive.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM [dbo].[RolePermissions]
        WHERE RoleID = @RoleID AND FeatureName = @FeatureName AND IsActive = 1)
    BEGIN
        UPDATE [dbo].[RolePermissions]
        SET CanView = @CanView,
            CanCreate = @CanCreate,
            CanEdit = @CanEdit,
            CanDelete = @CanDelete,
            UpdatedBy = @UpdatedBy,
            UpdatedAt = GETUTCDATE()
        WHERE RoleID = @RoleID AND FeatureName = @FeatureName AND IsActive = 1;

        SELECT CAST(ID AS INT) AS PermissionId FROM [dbo].[RolePermissions]
        WHERE RoleID = @RoleID AND FeatureName = @FeatureName AND IsActive = 1;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[RolePermissions] (
            RoleID, FeatureName, CanView, CanCreate, CanEdit, CanDelete, IsActive, CreatedAt, CreatedBy)
        VALUES (
            @RoleID, @FeatureName, @CanView, @CanCreate, @CanEdit, @CanDelete, 1, GETUTCDATE(), @CreatedBy);

        SELECT CAST(SCOPE_IDENTITY() AS INT) AS PermissionId;
    END
END
GO

-- SP: Available labor near user location with calendar slot tomorrow
CREATE OR ALTER PROCEDURE [dbo].[sp_AvailableLaborNearByTomorrow]
    @Latitude VARCHAR(250) = NULL,
    @Longitude VARCHAR(250) = NULL,
    @RadiusKm VARCHAR(30) = NULL,
    @AvailabilityStatus VARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @Tomorrow DATE = DATEADD(DAY, 1, CAST(GETDATE() AS DATE));
    DECLARE @Lat DECIMAL(10, 8) = TRY_CAST(NULLIF(LTRIM(RTRIM(@Latitude)), '') AS DECIMAL(10, 8));
    DECLARE @Lon DECIMAL(11, 8) = TRY_CAST(NULLIF(LTRIM(RTRIM(@Longitude)), '') AS DECIMAL(11, 8));
    DECLARE @Radius DECIMAL(10, 2) = TRY_CAST(NULLIF(LTRIM(RTRIM(@RadiusKm)), '') AS DECIMAL(10, 2));

    IF @Radius IS NULL
        SET @Radius = 10;

    ;WITH LaborNearbyTomorrow AS (
        SELECT
            l.ID AS LaborId,
            l.UserID AS UserId,
            p.FirstName,
            p.LastName,
            p.ProfilePicture,
            l.LaborTypeID,
            lt.TypeName AS LaborType,
            l.Specialization,
            l.ExperienceYears,
            l.Rating,
            l.TotalReviews,
            l.DailyRate,
            l.MinimumHours,
            l.MaximumHours,
            l.AvailabilityStatus,
            l.IsVerified,
            a.City,
            a.State,
            la.AvailableDate,
            CONVERT(VARCHAR(8), la.StartTime, 108) AS StartTime,
            CONVERT(VARCHAR(8), la.EndTime, 108) AS EndTime,
            CASE
                WHEN @Lat IS NOT NULL
                     AND @Lon IS NOT NULL
                     AND a.Latitude IS NOT NULL
                     AND a.Longitude IS NOT NULL
                THEN dbo.fn_CalculateDistance(@Lat, @Lon, a.Latitude, a.Longitude)
                ELSE NULL
            END AS DistanceKm
        FROM [dbo].[Labors] l
        INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
        LEFT JOIN [dbo].[Person] p ON u.PersonID = p.ID
        INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
        LEFT JOIN [dbo].[Addresses] a ON u.ID = a.UserID AND a.IsDefault = 1
        INNER JOIN (
            SELECT
                LaborID,
                AvailableDate,
                StartTime,
                EndTime,
                ROW_NUMBER() OVER (
                    PARTITION BY LaborID
                    ORDER BY StartTime
                ) AS rn
            FROM [dbo].[LaborAvailabilities]
            WHERE AvailableDate = @Tomorrow
              AND Status = N'Available'
        ) la ON la.LaborID = l.ID AND la.rn = 1
        WHERE l.IsActive = 1
          AND u.IsActive = 1
          AND (@AvailabilityStatus IS NULL OR l.AvailabilityStatus = @AvailabilityStatus)
    )
    SELECT
        LaborId,
        UserId,
        FirstName,
        LastName,
        ProfilePicture,
        LaborTypeID,
        LaborType,
        Specialization,
        ExperienceYears,
        Rating,
        TotalReviews,
        DailyRate,
        MinimumHours,
        MaximumHours,
        AvailabilityStatus,
        IsVerified,
        City,
        State,
        AvailableDate,
        StartTime,
        EndTime,
        DistanceKm
    FROM LaborNearbyTomorrow
    WHERE @Lat IS NULL
       OR @Lon IS NULL
       OR DistanceKm IS NULL
       OR DistanceKm <= @Radius
    ORDER BY
        CASE WHEN DistanceKm IS NULL THEN 1 ELSE 0 END,
        DistanceKm,
        Rating DESC,
        TotalReviews DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
-- ========== Labor IVR response (1=yes, 2=no) ==========
CREATE OR ALTER PROCEDURE [dbo].[sp_ProcessLaborIvrResponse]
    @LaborConfirmationID INT,
    @Digit NVARCHAR(5),
    @UpdatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    DECLARE @OrderID INT, @OrderItemID INT, @LaborID INT, @WorkDate DATE, @CurrentStatus NVARCHAR(50);
    SELECT @OrderID = lc.OrderID, @OrderItemID = lc.OrderItemID, @LaborID = lc.LaborID,
           @WorkDate = oi.PreferredWorkDate, @CurrentStatus = lc.Status
    FROM [dbo].[LaborConfirmations] lc
    INNER JOIN [dbo].[OrderItems] oi ON lc.OrderItemID = oi.ID
    WHERE lc.ID = @LaborConfirmationID;
    IF @OrderID IS NULL OR @CurrentStatus IN (N'Accepted', N'Declined')
    BEGIN ROLLBACK; SELECT 0 AS Success, NULL AS OrderID, NULL AS LaborID; RETURN; END
    IF @Digit = N'1'
    BEGIN
        UPDATE [dbo].[LaborConfirmations]
        SET Status = N'Accepted', ResponseDigit = N'1', RespondedAt = GETUTCDATE(), UpdatedAt = GETUTCDATE(),
            NextCallAt = DATEADD(year, 100, GETUTCDATE()) -- stop retries
        WHERE ID = @LaborConfirmationID;
        UPDATE [dbo].[OrderItems] SET ItemStatus = N'Assigned' WHERE ID = @OrderItemID;
        UPDATE la SET la.Status = N'Booked', la.UpdatedAt = GETDATE()
        FROM [dbo].[LaborAvailabilities] la
        WHERE la.LaborID = @LaborID AND la.AvailableDate = @WorkDate AND la.Status = N'OnHold';
    END
    ELSE IF @Digit = N'2'
    BEGIN
        UPDATE [dbo].[LaborConfirmations]
        SET Status = N'Declined', ResponseDigit = N'2', RespondedAt = GETUTCDATE(), UpdatedAt = GETUTCDATE(),
            NextCallAt = DATEADD(year, 100, GETUTCDATE())
        WHERE ID = @LaborConfirmationID;
        UPDATE [dbo].[OrderItems] SET ItemStatus = N'Declined' WHERE ID = @OrderItemID;
        UPDATE la SET la.Status = N'Available', la.UpdatedAt = GETDATE()
        FROM [dbo].[LaborAvailabilities] la
        WHERE la.LaborID = @LaborID AND la.AvailableDate = @WorkDate AND la.Status = N'OnHold';
    END
    ELSE
    BEGIN ROLLBACK; SELECT 0 AS Success, NULL AS OrderID, NULL AS LaborID; RETURN; END
    INSERT INTO [dbo].[OrderTracking] (OrderID, Status, Description, CreatedBy)
    VALUES (@OrderID, @Digit, N'Labor IVR response: ' + @Digit, @UpdatedBy);
    EXEC [dbo].[sp_RecalculateOrderConfirmationStatus] @OrderID;
    COMMIT TRANSACTION;
    SELECT 1 AS Success, @OrderID AS OrderID, @LaborID AS LaborID;
END
GO
-- ========== Pending IVR calls (retry job) ==========
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPendingLaborConfirmationsForCall]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT lc.ID AS LaborConfirmationID, lc.OrderID, lc.OrderItemID, lc.LaborID, lc.LaborMobile,
           lc.AttemptCount, o.OrderNumber, o.ScheduledDate,
           p.FirstName + ' ' + p.LastName AS EmployerName,
           a.City AS WorkArea, oi.WorkDescription
    FROM [dbo].[LaborConfirmations] lc
    INNER JOIN [dbo].[Orders] o ON lc.OrderID = o.ID
    INNER JOIN [dbo].[OrderItems] oi ON lc.OrderItemID = oi.ID
    INNER JOIN [dbo].[Users] eu ON o.EmployerID = eu.ID
    INNER JOIN [dbo].[Person] p ON eu.PersonID = p.ID
    INNER JOIN [dbo].[Addresses] a ON o.WorkAddressID = a.ID
    WHERE lc.Status = N'Pending'
      AND o.OrderStatus IN (N'Pending', N'PartiallyConfirmed')
      AND lc.NextCallAt <= GETUTCDATE()
      AND lc.AttemptCount < 12; -- max ~2 hours if every 10 min
END
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_MarkLaborConfirmationCallAttempt]
    @LaborConfirmationID INT,
    @ProviderCallId NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE [dbo].[LaborConfirmations]
    SET AttemptCount = AttemptCount + 1,
        LastCallAt = GETUTCDATE(),
        LastCallProviderId = @ProviderCallId,
        NextCallAt = DATEADD(MINUTE, 10, GETUTCDATE()),
        UpdatedAt = GETUTCDATE()
    WHERE ID = @LaborConfirmationID AND Status = N'Pending';
END
GO
-- ========== Employer order summary with per-labor status ==========
CREATE OR ALTER PROCEDURE [dbo].[sp_GetOrderLaborSummary]
    @OrderID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT o.ID AS OrderID, o.OrderNumber, o.OrderStatus, o.PaymentStatus, o.ScheduledDate, o.TotalAmount,
           o.CreatedAt, a.Street, a.City, a.State, a.ZipCode,
           (SELECT COUNT(*) FROM OrderItems WHERE OrderID = o.ID) AS TotalLabor,
           (SELECT COUNT(*) FROM OrderItems WHERE OrderID = o.ID AND ItemStatus = N'Assigned') AS ConfirmedLabor,
           (SELECT COUNT(*) FROM OrderItems WHERE OrderID = o.ID AND ItemStatus = N'PendingConfirmation') AS PendingLabor,
           (SELECT COUNT(*) FROM OrderItems WHERE OrderID = o.ID AND ItemStatus IN (N'Declined', N'Cancelled')) AS DeclinedLabor
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[Addresses] a ON o.WorkAddressID = a.ID
    WHERE o.ID = @OrderID
      AND (@UserID IS NULL OR o.EmployerID = @UserID
           OR EXISTS (SELECT 1 FROM OrderItems oi JOIN Labors l ON oi.LaborID = l.ID WHERE oi.OrderID = o.ID AND l.UserID = @UserID));
    SELECT oi.ID AS OrderItemID, oi.LaborID, oi.RequiredHours, oi.DailyRate, oi.TotalAmount,
           oi.WorkDescription, oi.ItemStatus, oi.PreferredWorkDate,
           p.FirstName + ' ' + p.LastName AS LaborName,
           CASE WHEN oi.ItemStatus = N'Assigned' THEN u.MobileNumber ELSE NULL END AS LaborMobile,
           lt.TypeName AS LaborType,
           lc.Status AS ConfirmationStatus, lc.AttemptCount, lc.RespondedAt,
           CASE WHEN lr.ID IS NOT NULL THEN 1 ELSE 0 END AS HasReview,
           lr.ID AS ReviewId,
           lr.Rating AS ReviewRating,
           lr.Comment AS ReviewComment
    FROM [dbo].[OrderItems] oi
    INNER JOIN [dbo].[Labors] l ON oi.LaborID = l.ID
    INNER JOIN [dbo].[Users] u ON l.UserID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    INNER JOIN [dbo].[LaborTypes] lt ON l.LaborTypeID = lt.ID
    LEFT JOIN [dbo].[LaborConfirmations] lc ON lc.OrderItemID = oi.ID
    LEFT JOIN [dbo].[LaborReviews] lr ON lr.OrderItemID = oi.ID AND lr.IsActive = 1
    WHERE oi.OrderID = @OrderID
    ORDER BY oi.ID;
END
GO

-- ========== Payment (Razorpay / Mock) ==========

CREATE OR ALTER PROCEDURE [dbo].[sp_GetOrderPaymentSummary]
    @OrderID INT,
    @EmployerID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Orders] WHERE ID = @OrderID AND EmployerID = @EmployerID)
    BEGIN
        RAISERROR(N'Order not found.', 16, 1);
        RETURN;
    END

    DECLARE @PaymentStatus NVARCHAR(50);
    DECLARE @OrderNumber NVARCHAR(50);
    DECLARE @LaborAmount DECIMAL(10,2) = 0;
    DECLARE @PayableItemCount INT = 0;
    DECLARE @CompletedItemCount INT = 0;
    DECLARE @CanPay BIT = 0;

    SELECT @PaymentStatus = PaymentStatus, @OrderNumber = OrderNumber
    FROM [dbo].[Orders] WHERE ID = @OrderID;

    SELECT @PayableItemCount = COUNT(*)
    FROM [dbo].[OrderItems]
    WHERE OrderID = @OrderID
      AND ItemStatus NOT IN (N'Declined', N'Cancelled');

    SELECT @CompletedItemCount = COUNT(*),
           @LaborAmount = ISNULL(SUM(TotalPrice), 0)
    FROM [dbo].[OrderItems]
    WHERE OrderID = @OrderID
      AND ItemStatus = N'Completed';

    IF @PayableItemCount > 0 AND @CompletedItemCount = @PayableItemCount
        AND @PaymentStatus <> N'Paid'
        SET @CanPay = 1;

    SELECT
        @OrderID AS OrderId,
        @OrderNumber AS OrderNumber,
        @PaymentStatus AS PaymentStatus,
        @CanPay AS CanPay,
        @PayableItemCount AS PayableItemCount,
        @CompletedItemCount AS CompletedItemCount,
        @LaborAmount AS LaborAmount,
        CAST(0 AS DECIMAL(10,2)) AS PlatformFee,
        CAST(0 AS DECIMAL(10,2)) AS DiscountAmount,
        CAST(NULL AS NVARCHAR(50)) AS CouponCode,
        @LaborAmount AS TotalAmount,
        N'INR' AS Currency,
        lp.ID AS LastPaymentId,
        lp.Status AS LastPaymentStatus,
        lp.ProviderOrderId AS LastProviderOrderId,
        lp.PaidAt AS LastPaidAt
    FROM (SELECT 1 AS x) AS dummy
    OUTER APPLY (
        SELECT TOP 1 p.ID, p.Status, p.ProviderOrderId, p.PaidAt
        FROM [dbo].[Payments] p
        WHERE p.OrderID = @OrderID
        ORDER BY p.ID DESC
    ) lp;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CreatePaymentRecord]
    @OrderID INT,
    @EmployerID INT,
    @LaborAmount DECIMAL(10,2),
    @PlatformFee DECIMAL(10,2),
    @DiscountAmount DECIMAL(10,2),
    @CouponCode NVARCHAR(50) = NULL,
    @TotalAmount DECIMAL(10,2),
    @Provider NVARCHAR(30),
    @ProviderOrderId NVARCHAR(100),
    @PaymentID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Orders] WHERE ID = @OrderID AND EmployerID = @EmployerID)
    BEGIN
        RAISERROR(N'Order not found.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM [dbo].[Orders] WHERE ID = @OrderID AND PaymentStatus = N'Paid')
    BEGIN
        RAISERROR(N'Order is already paid.', 16, 1);
        RETURN;
    END

    INSERT INTO [dbo].[Payments] (
        OrderID, EmployerID, LaborAmount, PlatformFee, DiscountAmount, CouponCode,
        TotalAmount, Provider, ProviderOrderId, Status
    )
    VALUES (
        @OrderID, @EmployerID, @LaborAmount, @PlatformFee, @DiscountAmount, @CouponCode,
        @TotalAmount, @Provider, @ProviderOrderId, N'Created'
    );

    SET @PaymentID = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CompletePayment]
    @PaymentID INT,
    @EmployerID INT,
    @ProviderPaymentId NVARCHAR(100),
    @ProviderSignature NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OrderID INT;
    DECLARE @TotalAmount DECIMAL(10,2);
    DECLARE @OrderNumber NVARCHAR(50);

    SELECT @OrderID = p.OrderID, @TotalAmount = p.TotalAmount
    FROM [dbo].[Payments] p
    WHERE p.ID = @PaymentID AND p.EmployerID = @EmployerID AND p.Status = N'Created';

    IF @OrderID IS NULL
    BEGIN
        RAISERROR(N'Payment not found or already completed.', 16, 1);
        RETURN;
    END

    UPDATE [dbo].[Payments]
    SET Status = N'Paid',
        ProviderPaymentId = @ProviderPaymentId,
        ProviderSignature = @ProviderSignature,
        PaidAt = GETUTCDATE(),
        UpdatedAt = GETUTCDATE()
    WHERE ID = @PaymentID;

    UPDATE [dbo].[Orders]
    SET PaymentStatus = N'Paid',
        TotalAmount = @TotalAmount,
        UpdatedAt = GETUTCDATE()
    WHERE ID = @OrderID;

    SELECT @OrderID = o.ID, @OrderNumber = o.OrderNumber
    FROM [dbo].[Orders] o WHERE o.ID = @OrderID;

    SELECT @PaymentID AS PaymentId, @OrderID AS OrderId, @OrderNumber AS OrderNumber, @TotalAmount AS AmountPaid;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetEmployerContactForOrder]
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.ID AS UserId, u.MobileNumber, p.FirstName, p.LastName
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[Users] u ON o.EmployerID = u.ID
    INNER JOIN [dbo].[Person] p ON u.PersonID = p.ID
    WHERE o.ID = @OrderID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_InsertNotificationLog]
    @UserID INT = NULL,
    @Mobile NVARCHAR(20),
    @Channel NVARCHAR(20),
    @TemplateKey NVARCHAR(100),
    @MessageBody NVARCHAR(2000),
    @Status NVARCHAR(50),
    @ProviderMessageId NVARCHAR(100) = NULL,
    @ErrorMessage NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[NotificationLogs] (UserID, Mobile, Channel, TemplateKey, MessageBody, Status, ProviderMessageId, ErrorMessage)
    VALUES (@UserID, @Mobile, @Channel, @TemplateKey, @MessageBody, @Status, @ProviderMessageId, @ErrorMessage);
END
GO

-- ========== Labor reviews (employer rates completed work) ==========

CREATE OR ALTER PROCEDURE [dbo].[sp_RecalculateLaborRating]
    @LaborId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AvgRating DECIMAL(3, 2);
    DECLARE @TotalReviews INT;

    SELECT
        @AvgRating = AVG(CAST(Rating AS DECIMAL(3, 2))),
        @TotalReviews = COUNT(*)
    FROM [dbo].[LaborReviews]
    WHERE LaborID = @LaborId AND IsActive = 1;

    UPDATE [dbo].[Labors]
    SET Rating = COALESCE(@AvgRating, 0),
        TotalReviews = COALESCE(@TotalReviews, 0),
        UpdatedAt = GETUTCDATE()
    WHERE ID = @LaborId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_AddLaborReview]
    @OrderItemId INT,
    @EmployerId INT,
    @LaborId INT,
    @Rating INT,
    @Comment NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    DECLARE @ItemStatus NVARCHAR(50);
    DECLARE @ItemLaborId INT;
    DECLARE @OrderEmployerId INT;

    IF @Rating < 1 OR @Rating > 5
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'Rating must be between 1 and 5.', 16, 1);
        RETURN;
    END

    SELECT
        @ItemStatus = oi.ItemStatus,
        @ItemLaborId = oi.LaborID,
        @OrderEmployerId = o.EmployerID
    FROM [dbo].[OrderItems] oi
    INNER JOIN [dbo].[Orders] o ON oi.OrderID = o.ID
    WHERE oi.ID = @OrderItemId;

    IF @ItemStatus IS NULL OR @OrderEmployerId <> @EmployerId
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'Order item not found.', 16, 1);
        RETURN;
    END

    IF @ItemStatus <> N'Completed'
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'You can only review workers after their work is completed.', 16, 1);
        RETURN;
    END

    IF @ItemLaborId <> @LaborId
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'Labor does not match this order item.', 16, 1);
        RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM [dbo].[LaborReviews]
        WHERE OrderItemID = @OrderItemId AND IsActive = 1
    )
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'You have already reviewed this worker for this order.', 16, 1);
        RETURN;
    END

    INSERT INTO [dbo].[LaborReviews] (
        OrderItemID, EmployerID, LaborID, Rating, Comment, IsActive, CreatedAt
    )
    VALUES (
        @OrderItemId, @EmployerId, @LaborId, @Rating, @Comment, 1, GETUTCDATE()
    );

    EXEC [dbo].[sp_RecalculateLaborRating] @LaborId;

    COMMIT TRANSACTION;

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReviewId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_UpdateReview]
    @ReviewId INT,
    @EmployerId INT,
    @Rating INT,
    @Comment NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LaborId INT;

    SELECT @LaborId = LaborID
    FROM [dbo].[LaborReviews]
    WHERE ID = @ReviewId AND EmployerID = @EmployerId AND IsActive = 1;

    IF @LaborId IS NULL
    BEGIN
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    UPDATE [dbo].[LaborReviews]
    SET Rating = @Rating,
        Comment = @Comment
    WHERE ID = @ReviewId AND EmployerID = @EmployerId AND IsActive = 1;

    EXEC [dbo].[sp_RecalculateLaborRating] @LaborId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_DeleteReview]
    @ReviewId INT,
    @EmployerId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LaborId INT;

    SELECT @LaborId = LaborID
    FROM [dbo].[LaborReviews]
    WHERE ID = @ReviewId AND EmployerID = @EmployerId AND IsActive = 1;

    IF @LaborId IS NULL
    BEGIN
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    UPDATE [dbo].[LaborReviews]
    SET IsActive = 0
    WHERE ID = @ReviewId AND EmployerID = @EmployerId;

    EXEC [dbo].[sp_RecalculateLaborRating] @LaborId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
--create or update labor types
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateorupdateLaborTypes]
  @TypeName varchar(100),
  @Description varchar(255)=null,
  @DailyRate decimal(10,2)=null,
  @isActive bit =0,
  @CreatedBy int =null,
  @updatedBy int =null
As
 begin
  set nocount on;

   if exists(select top 1 1 from LaborTypes where TypeName=@TypeName)
   begin
     update LaborTypes set 
	 Description =@Description,
	 DailyRate=@DailyRate,
	 IsActive=@isActive,
	 UpdatedAt=GETDATE(),
	 UpdatedBy=@updatedBy
	 where TypeName=@TypeName
   select 1 as NewTypeId
   end
   else
   begin 
     insert into LaborTypes(TypeName,Description,DailyRate,IsActive,CreatedAt,CreatedBy)
	 values(@TypeName,@Description,@DailyRate,1,GETDATE(),@CreatedBy)
	 select cast(SCOPE_IDENTITY() as int) as NewTypeId
   end
  end

