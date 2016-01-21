SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadMatchingGradeRanges') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMatchingGradeRanges AS SELECT 1')
GO

ALTER PROCEDURE LoadMatchingGradeRanges
@OriginID INT,
@IsRegulated BIT,
@Score DECIMAL(18, 6),
@LoanSourceID INT,
@IsFirstLoan BIT
AS
BEGIN
	SELECT DISTINCT
		r.GradeRangeID
	FROM
		I_GradeRange r
		INNER JOIN I_SubGrade s ON r.SubGradeID = s.SubGradeID
		INNER JOIN I_ProductSubType st
			ON st.LoanSourceID = r.LoanSourceID
			AND st.IsRegulated = @IsRegulated
	WHERE
		r.OriginID = @OriginID
		AND
		s.MinScore <= @Score AND @Score <= s.MaxScore
		AND
		r.LoanSourceID = @LoanSourceID
		AND
		r.IsFirstLoan = @IsFirstLoan
		AND
		r.IsActive = 1
END
GO
