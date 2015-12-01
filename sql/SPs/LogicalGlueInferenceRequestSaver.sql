SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueInferenceRequestSaver') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueInferenceRequestSaver AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueInferenceRequestSaver
@CustomerID INT,
@CompanyID INT,
@Now DATETIME,
@UniqueID UNIQUEIDENTIFIER,
@RequestText NVARCHAR(MAX),
@EquifaxData NVARCHAR(MAX),
@MonthlyPayment DECIMAL(18, 0),
@CompanyRegistrationNumber NVARCHAR(32),
@FirstName NVARCHAR(250),
@LastName NVARCHAR(250),
@DateOfBirth DATETIME
AS
BEGIN
	DECLARE @ServiceLogID BIGINT = NULL

	BEGIN TRANSAÑTION

	INSERT INTO MP_ServiceLog(ServiceType, InsertDate, RequestData, CustomerId, CompanyID, CompanyRefNum, Firstname, Surname, DateOfBirth)

	COMMIT TRANSACTION
END
GO
