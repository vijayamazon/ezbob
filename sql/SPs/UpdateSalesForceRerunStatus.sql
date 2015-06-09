IF OBJECT_ID('UpdateSalesForceRerunStatus') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateSalesForceRerunStatus AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateSalesForceRerunStatus
@SalesForceLogID INT,
@Now DATETIME,
@RerunSuccess BIT ,
@Error NVARCHAR(1000)
AS
BEGIN
UPDATE SalesForceLog SET RerunDate = @Now, RerunSuccess = @RerunSuccess, Error = @Error WHERE SalesForceLogID = @SalesForceLogID
END 
GO
