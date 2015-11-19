IF OBJECT_ID('GetSmsSender') IS NULL
	EXECUTE('CREATE PROCEDURE GetSmsSender AS SELECT 1')

GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetSmsSender
	@PhoneWithPrefix NVARCHAR(20),
	@Phone NVARCHAR(20)
AS
BEGIN
	DECLARE @UserID INT = (SELECT TOP 1 s.UserId
						  FROM SmsMessage s
						  WHERE s.[To] = @PhoneWithPrefix 
						  AND s.UserId IS NOT NULL
						  ORDER BY s.Id DESC)
						  
	IF @UserID IS NULL
	BEGIN
		SET @UserID = (SELECT TOP 1 c.Id 
				      FROM Customer c 
				      WHERE c.MobilePhone = @Phone OR c.DaytimePhone = @Phone)
	END
					  
	SELECT @UserID AS UserID					  
						
END
GO