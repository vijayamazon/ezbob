SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetIsConsumerDataUpdated') IS NULL
	EXECUTE('CREATE PROCEDURE GetIsConsumerDataUpdated AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[GetIsConsumerDataUpdated] 
	(@CustomerId INT,
	 @DirectorId INT = NULL, 
	 @Today DATE)
AS
BEGIN
	DECLARE @LastUpdateTime DATETIME
			
	IF @DirectorId IS NULL OR @DirectorId = 0
	BEGIN
		SET @LastUpdateTime = (SELECT max(InsertDate) FROM MP_ServiceLog WHERE CustomerId = @CustomerId AND (DirectorId IS NULL OR DirectorId = 0) AND ServiceType='Consumer Request')
	END
	ELSE
	BEGIN
		SET @LastUpdateTime = (SELECT max(InsertDate) FROM MP_ServiceLog WHERE CustomerId = @CustomerId AND DirectorId = @DirectorId AND ServiceType='Consumer Request')
	END
		
	IF datediff(day, @Today, @LastUpdateTime) = 0
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated
END

GO

