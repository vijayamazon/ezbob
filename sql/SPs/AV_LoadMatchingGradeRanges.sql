SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_LoadMatchingGradeRanges') IS NULL
	EXECUTE('CREATE PROCEDURE AV_LoadMatchingGradeRanges AS SELECT 1')
GO

ALTER PROCEDURE AV_LoadMatchingGradeRanges
@CustomerID INT,
@Score DECIMAL(18, 6),
@Regulated BIT,
@ProcessingDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	;WITH subtypes AS (
		SELECT DISTINCT
			st.ProductSubtypeID
		FROM
			I_ProductSubType st
			INNER JOIN LoanSource ls
				ON st.LoanSourceID = ls.LoanSourceID
				AND ls.IsDefault = 1
		WHERE
			st.OriginID = (SELECT c.OriginID FROM Customer c WHERE Id = @CustomerID)
			AND
			st.IsRegulated = @Regulated
	)
	SELECT DISTINCT
		st.ProductSubtypeID,
		r.GradeRangeID
	FROM
		I_GradeRange r
		INNER JOIN LoanSource ls
			ON ls.LoanSourceID = r.LoanSourceID
			AND ls.IsDefault = 1
		INNER JOIN I_SubGrade s
			ON r.SubGradeID = s.SubGradeID
			AND s.MinScore <= @Score AND @Score <= s.MaxScore
		INNER JOIN subtypes st ON 1 = 1
	WHERE
		r.OriginID = (SELECT c.OriginID FROM Customer c WHERE Id = @CustomerID)
		AND
		r.IsFirstLoan = (CASE WHEN (SELECT COUNT(DISTINCT Id) FROM Loan l WHERE l.CustomerId = @CustomerID AND l.[Date] <= @ProcessingDate) > 0 THEN 1 ELSE 0 END)
		AND
		r.IsActive = 1
END
GO
