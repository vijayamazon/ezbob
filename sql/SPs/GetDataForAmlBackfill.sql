IF OBJECT_ID('GetDataForAmlBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForAmlBackfill AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetDataForAmlBackfill
	(@CustomerId INT,
	 @ServiceLogId BIGINT)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE
		@FirstName NVARCHAR(250),
		@MiddleName NVARCHAR(250),
		@Surname NVARCHAR(250),
		@Postcode VARCHAR(200),
		@ExistingAmlResult NVARCHAR(100),
		@IsLastEntryForCustomer BIT
		
	SELECT @Postcode = Postcode FROM CustomerAddress WHERE addressType = 1 AND CustomerId = @CustomerId
	
	SELECT
		@ExistingAmlResult = AMLResult,
		@FirstName = FirstName,
		@MiddleName = MiddleInitial,
		@Surname = Surname
	FROM
		Customer
	WHERE
		Id = @CustomerId
	
	IF EXISTS (SELECT 1 FROM MP_ServiceLog WHERE CustomerId = @CustomerId AND Id > @ServiceLogId)
		SET @IsLastEntryForCustomer = 0
	ELSE
		SET @IsLastEntryForCustomer = 1
	
	SELECT
		InsertDate,
		ResponseData,
		@FirstName AS FirstName,
		@MiddleName AS MiddleName,
		@Surname AS Surname,
		@Postcode AS Postcode,
		@ExistingAmlResult AS ExistingAmlResult,
		@IsLastEntryForCustomer AS IsLastEntryForCustomer
	FROM
		MP_ServiceLog
	WHERE
		Id = @ServiceLogId	
END
GO
