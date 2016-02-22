SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadMatchingGradeRanges') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMatchingGradeRanges AS SELECT 1')
GO

ALTER PROCEDURE LoadMatchingGradeRanges
@CustomerID INT,
@CompanyID INT,
@Now DATETIME
AS
BEGIN
	IF @CompanyID IS NULL
		SET @CompanyID = dbo.udfGetCustomerCompanyID(@CustomerID, @Now)

	------------------------------------------------------------------------------

	DECLARE @OriginID INT = (SELECT OriginID FROM Customer WHERE Id = @CustomerID)

	------------------------------------------------------------------------------

	DECLARE @LoanSourceID INT = (SELECT LoanSourceID FROM dbo.udfGetLoanSource(0, @OriginID))

	------------------------------------------------------------------------------

	DECLARE @LoanCount INT = ISNULL((
		SELECT COUNT(DISTINCT l.Id)
		FROM Loan l
		INNER JOIN CashRequests r ON l.RequestCashId = r.Id
		WHERE r.IdCustomer = @CustomerID
		AND l.[Date] < @Now
	), 0)

	------------------------------------------------------------------------------

	DECLARE @IsRegulated BIT = ISNULL((
		SELECT t.IsRegulated
		FROM TypeOfBusiness t
		INNER JOIN Company c
			ON t.Name = c.TypeOfBusiness
			AND c.Id = @CompanyID
	), 1)

	------------------------------------------------------------------------------

	DECLARE @Score DECIMAL(18, 6) = (
		SELECT TOP 1 Score
		FROM CustomerLogicalGlueHistory
		WHERE CustomerID = @CustomerID
		AND CompanyID = @CompanyID
		AND SetTime <= @Now
		ORDER BY SetTime DESC, EntryID DESC
	)

	------------------------------------------------------------------------------

	DECLARE @IsFirstLoan BIT = (CASE WHEN @LoanCount <= 0 THEN 1 ELSE 0 END)

	------------------------------------------------------------------------------

	;WITH subtypes AS (
		SELECT
			st.ProductSubTypeID
		FROM
			I_ProductSubType st
		WHERE
			st.OriginID = @OriginID
			AND
			st.LoanSourceID = @LoanSourceID
			AND
			st.IsRegulated = @IsRegulated
	), grade_ranges AS (
		SELECT DISTINCT
			r.GradeRangeID,
			r.SubGradeID,
			st.ProductSubTypeID
		FROM
			I_GradeRange r
			INNER JOIN subtypes st ON 1 = 1
		WHERE
			r.OriginID = @OriginID
			AND
			r.LoanSourceID = @LoanSourceID
			AND
			r.IsFirstLoan = @IsFirstLoan
			AND
			r.IsActive = 1
	), sub_grades AS (
		SELECT
			SubGradeID = s.SubGradeID
		FROM
			I_SubGrade s
		WHERE
			@Score IS NOT NULL
			AND
			s.MinScore <= @Score AND @Score <= s.MaxScore

		UNION

		SELECT
			SubGradeID = NULL
		FROM
			I_SubGrade s
		WHERE
			@Score IS NULL
	) SELECT
		r.GradeRangeID,
		r.ProductSubTypeID
	FROM
		grade_ranges r
		INNER JOIN sub_grades s ON (
			(s.SubGradeID IS NULL AND r.SubGradeID IS NULL)
			OR
			(s.SubGradeID IS NOT NULL AND r.SubGradeID = s.SubGradeID)
		)
END
GO
