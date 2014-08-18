IF OBJECT_ID('UpdateServiceLogData') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateServiceLogData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateServiceLogData
 @ServiceLogId BIGINT
,@Firstname NVARCHAR(50)
,@Surname NVARCHAR(50)
,@Postcode NVARCHAR(50)
,@DateOfBirth DATETIME
,@Score INT = NULL
,@CustomerId INT = NULL
,@DirectorId INT = NULL
AS
BEGIN
	UPDATE MP_ServiceLog SET 
		Firstname=@Firstname,
		Surname=@Surname, 
		DateOfBirth=@DateOfBirth, 
		Postcode=@Postcode
	WHERE Id=@ServiceLogId 
	
	IF @DirectorId IS NOT NULL AND @Score IS NOT NULL
	BEGIN
		UPDATE Director SET ExperianConsumerScore=@Score WHERE id=@DirectorId	
	END
	
	IF @CustomerId IS NOT NULL AND @Score IS NOT NULL
	BEGIN
		UPDATE Customer SET ExperianConsumerScore=@Score WHERE Id=@CustomerId
	END
END
GO



