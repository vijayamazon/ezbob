SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveServiceLogEntry') IS NULL
	EXECUTE('CREATE PROCEDURE SaveServiceLogEntry AS SELECT 1')
GO

ALTER PROCEDURE SaveServiceLogEntry
@CustomerID INT,
@DirectorID INT,
@InsertDate DATETIME,
@ServiceType NVARCHAR(500),
@RequestData NVARCHAR(50),
@ResponseData NVARCHAR(MAX),
@CompanyRefNum NVARCHAR(50),
@Firstname NVARCHAR(50),
@Surname NVARCHAR(50),
@DateOfBirth DATETIME,
@Postcode NVARCHAR(50)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM Customer WHERE Id = @CustomerID)
		SET @CustomerID = NULL

	IF NOT EXISTS (SELECT * FROM Director WHERE Id = @DirectorID)
		SET @DirectorID = NULL

	INSERT INTO MP_ServiceLog (
		ServiceType, InsertDate, RequestData, ResponseData, CustomerId,
		DirectorId, CompanyRefNum, Firstname, Surname, DateOfBirth, Postcode
	) VALUES (
		@ServiceType, @InsertDate, @RequestData, @ResponseData, @CustomerID,
		@DirectorID, @CompanyRefNum, @Firstname, @Surname, @DateOfBirth, @Postcode
	)

	DECLARE @ServiceLogID BIGINT = SCOPE_IDENTITY()

	SELECT
		ServiceLogID = @ServiceLogID,
		CustomerID = @CustomerID,
		DirectorID = @DirectorID
END
GO
