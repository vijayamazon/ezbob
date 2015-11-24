SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UpdateDailyLoanStats') IS NOT NULL
	DROP PROCEDURE UpdateDailyLoanStats
GO

IF TYPE_ID('UpdateDailyLoanStatsPkg') IS NOT NULL
	DROP TYPE UpdateDailyLoanStatsPkg
GO

CREATE TYPE UpdateDailyLoanStatsPkg AS TABLE (
	LoanID INT NOT NULL,
	TheDate DATETIME NOT NULL,
	EarnedInterestByPeriods DECIMAL(18, 2) NOT NULL,
	EarnedInterestBySomeDate DECIMAL(18, 2) NOT NULL
)
GO

CREATE PROCEDURE UpdateDailyLoanStats
@DaysToKeep INT,
@Now DATETIME,
@UpdatePkg UpdateDailyLoanStatsPkg READONLY
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	IF @DaysToKeep > 0
		DELETE FROM LoanDailyStats WHERE TheDate < DATEADD(day, -@DaysToKeep, @Now)

	------------------------------------------------------------------------------

	MERGE
		LoanDailyStats dls -- this is target
	USING
		@UpdatePkg pkg -- this is source
	ON
		dls.LoanID = pkg.LoanID
		AND
		CONVERT(DATE, dls.TheDate) = CONVERT(DATE, pkg.TheDate)
	WHEN MATCHED THEN -- found in both source and target => update target from source
		UPDATE SET
			dls.EarnedInterestByPeriods = pkg.EarnedInterestByPeriods,
			dls.EarnedInterestBySomeDate = pkg.EarnedInterestBySomeDate
	WHEN NOT MATCHED BY TARGET THEN -- found in source but not found in target => insert into target
		INSERT (LoanID, TheDate, EarnedInterestByPeriods, EarnedInterestBySomeDate)
			VALUES (pkg.LoanID, CONVERT(DATE, pkg.TheDate), pkg.EarnedInterestByPeriods, EarnedInterestBySomeDate)
	;

	------------------------------------------------------------------------------
END
GO
