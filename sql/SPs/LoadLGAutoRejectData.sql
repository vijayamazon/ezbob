SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadLGAutoRejectData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadLGAutoRejectData AS SELECT 1')
GO

ALTER PROCEDURE LoadLGAutoRejectData
@CustomerID INT,
@CompanyID INT,
@Now DATETIME,
@OriginID INT OUTPUT,
@TypeOfBusinessName NVARCHAR(50) OUTPUT,
@LoanSourceID INT OUTPUT,
@LoanCount INT OUTPUT
AS
BEGIN
	SET @OriginID = NULL
	SET @TypeOfBusinessName = NULL
	SET @LoanSourceID = NULL

	------------------------------------------------------------------------------

	SELECT
		@OriginID = OriginID
	FROM
		Customer
	WHERE
		Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		@TypeOfBusinessName = TypeOfBusiness
	FROM
		Company
	WHERE
		Id = @CompanyID

	------------------------------------------------------------------------------

	SELECT
		@LoanSourceID = LoanSourceID
	FROM
		dbo.udfGetLoanSource(0, @OriginID)

	------------------------------------------------------------------------------

	SELECT
		@LoanCount = COUNT(DISTINCT l.Id)
	FROM
		Loan l
		INNER JOIN CashRequests r ON l.RequestCashId = r.Id
	WHERE
		r.IdCustomer = @CustomerID
		AND
		l.[Date] < @Now 
END
GO
