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
	) SELECT DISTINCT
		r.GradeRangeID,
		st.ProductSubTypeID
	FROM
		I_GradeRange r
		INNER JOIN I_SubGrade s ON r.SubGradeID = s.SubGradeID
		INNER JOIN subtypes st ON 1 = 1
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
