SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveInferenceRequest') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueSaveInferenceRequest AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueSaveInferenceRequest
@CustomerID INT,
@CompanyID INT,
@IsTryOut BIT,
@Now DATETIME,
@UniqueID UNIQUEIDENTIFIER,
@RequestText NVARCHAR(MAX),
@EquifaxData NVARCHAR(MAX),
@MonthlyPayment DECIMAL(18, 0),
@CompanyRegistrationNumber NVARCHAR(50),
@FirstName NVARCHAR(250),
@LastName NVARCHAR(250),
@DateOfBirth DATETIME,
@Postcode NVARCHAR(255),
@HouseNumber NVARCHAR(255)
AS
BEGIN
	DECLARE @ServiceLogID BIGINT = NULL

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	INSERT INTO MP_ServiceLog (
		ServiceType, InsertDate, RequestData,
		CustomerId, CompanyID, CompanyRefNum,
		Firstname, Surname, DateOfBirth, Postcode
	) VALUES (
		'LogicalGlue', @Now, @RequestText,
		@CustomerID, @CompanyID, @CompanyRegistrationNumber,
		@FirstName, @LastName, @DateOfBirth, @Postcode
	)

	------------------------------------------------------------------------------

	SET @ServiceLogiD = SCOPE_IDENTITY()

	------------------------------------------------------------------------------

	INSERT INTO LogicalGlueRequests (ServiceLogID, IsTryOut, UniqueID, MonthlyRepayment, EquifaxData, HouseNumber)
		VALUES (@ServiceLogID, @IsTryOut, @UniqueID, @MonthlyPayment, @EquifaxData, @HouseNumber)

	------------------------------------------------------------------------------

	COMMIT TRANSACTION	

	------------------------------------------------------------------------------

	SELECT ServiceLogID = @ServiceLogID
END
GO
