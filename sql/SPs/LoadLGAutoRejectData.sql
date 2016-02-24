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
@LoanSourceID INT OUTPUT,
@LoanCount INT OUTPUT,
@IsRegulated BIT OUTPUT,
@AutoDecisionInternalLogic BIT OUTPUT,
@TypeOfBusinessName NVARCHAR(50) OUTPUT
AS
BEGIN
	SET @OriginID = NULL
	SET @TypeOfBusinessName = NULL
	SET @LoanSourceID = NULL
	SET @IsRegulated = NULL
	SET @AutoDecisionInternalLogic = NULL

	------------------------------------------------------------------------------

	SELECT
		@OriginID = OriginID
	FROM
		Customer
	WHERE
		Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		@TypeOfBusinessName = c.TypeOfBusiness,
		@IsRegulated = t.IsRegulated
	FROM
		Company c
		INNER JOIN TypeOfBusiness t ON c.TypeOfBusiness = t.Name
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

	------------------------------------------------------------------------------

	SELECT
		@AutoDecisionInternalLogic = AutoDecisionInternalLogic
	FROM
		I_ProductSubType
	WHERE
		OriginID = @OriginID
		AND
		LoanSourceID = @LoanSourceID
		AND
		IsRegulated = @IsRegulated

	SET @AutoDecisionInternalLogic = ISNULL(@AutoDecisionInternalLogic, 1)
END
GO
