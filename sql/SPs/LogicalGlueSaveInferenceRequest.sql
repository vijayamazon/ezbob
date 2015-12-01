SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveInferenceRequest') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueSaveInferenceRequest AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueSaveInferenceRequest
@CustomerID INT,
@CompanyID INT,
@Now DATETIME,
@UniqueID UNIQUEIDENTIFIER,
@RequestText NVARCHAR(MAX),
@EquifaxData NVARCHAR(MAX),
@MonthlyPayment DECIMAL(18, 0),
@CompanyRegistrationNumber NVARCHAR(50),
@FirstName NVARCHAR(250),
@LastName NVARCHAR(250),
@DateOfBirth DATETIME
AS
BEGIN
	DECLARE @ServiceLogID BIGINT = NULL

	------------------------------------------------------------------------------

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	INSERT INTO MP_ServiceLog (
		ServiceType, InsertDate, RequestData,
		CustomerId, CompanyID, CompanyRefNum,
		Firstname, Surname, DateOfBirth
	) VALUES (
		'LogicalGlue', @Now, @RequestText,
		@CustomerID, @CompanyID, @CompanyRegistrationNumber,
		@FirstName, @LastName, @DateOfBirth
	)

	------------------------------------------------------------------------------

	SET @ServiceLogiD = SCOPE_IDENTITY()
	
	------------------------------------------------------------------------------

	INSERT INTO LogicalGlueRequests (ServiceLogID, UniqueID, MonthlyRepayment, EquifaxData)
		VALUES (@ServiceLogID, @UniqueID, @MonthlyPayment, @EquifaxData)
	
	------------------------------------------------------------------------------

	COMMIT TRANSACTION	

	------------------------------------------------------------------------------

	SELECT ServiceLogID = @ServiceLogID
END
GO
