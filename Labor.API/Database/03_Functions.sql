-- Labor Management System Functions
USE LaborManagementDB;
GO

-- Function to calculate distance between two geographical points
CREATE FUNCTION dbo.fn_CalculateDistance(
    @Lat1 DECIMAL(10, 8),
    @Lon1 DECIMAL(11, 8),
    @Lat2 DECIMAL(10, 8),
    @Lon2 DECIMAL(11, 8)
)
RETURNS DECIMAL(10, 2)
AS
BEGIN
    DECLARE @Distance DECIMAL(10, 2);
    
    -- Handle NULL values
    IF @Lat1 IS NULL OR @Lon1 IS NULL OR @Lat2 IS NULL OR @Lon2 IS NULL
        RETURN NULL;
    
    -- Haversine formula
    DECLARE @R DECIMAL(10, 2) = 6371; -- Earth's radius in kilometers
    DECLARE @dLat DECIMAL(10, 8) = RADIANS(@Lat2 - @Lat1);
    DECLARE @dLon DECIMAL(10, 8) = RADIANS(@Lon2 - @Lon1);
    DECLARE @a DECIMAL(10, 8);
    
    SET @a = SIN(@dLat/2) * SIN(@dLat/2) + 
             COS(RADIANS(@Lat1)) * COS(RADIANS(@Lat2)) * 
             SIN(@dLon/2) * SIN(@dLon/2);
    
    DECLARE @c DECIMAL(10, 8) = 2 * ATN2(SQRT(@a), SQRT(1-@a));
    SET @Distance = @R * @c;
    
    RETURN @Distance;
END
GO 