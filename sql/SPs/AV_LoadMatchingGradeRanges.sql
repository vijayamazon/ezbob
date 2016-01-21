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

	SELECT DISTINCT
		r.GradeRangeID
	FROM
		I_GradeRange r
		INNER JOIN CustomerOrigin org
			ON r.OriginID = org.CustomerOriginID
			AND org.CustomerOriginID = (SELECT c.OriginID FROM Customer c WHERE Id = @CustomerID)
		INNER JOIN LoanSource ls
			ON ls.LoanSourceID = r.LoanSourceID
			AND ls.IsDefault = 1
		INNER JOIN I_ProductSubType st
			ON st.LoanSourceID = ls.LoanSourceID
			AND st.IsRegulated = @Regulated
		INNER JOIN I_SubGrade s
			ON r.SubGradeID = s.SubGradeID
			AND s.MinScore <= @Score AND @Score <= s.MaxScore
	WHERE
		r.IsFirstLoan = (CASE WHEN (SELECT COUNT(DISTINCT Id) FROM Loan l WHERE l.CustomerId = @CustomerID AND l.[Date] <= @ProcessingDate) > 0 THEN 1 ELSE 0 END)
		AND
		r.IsActive = 1
END
GO
