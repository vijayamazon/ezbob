IF OBJECT_ID(N'[dbo].udfEarnedInterest') IS NOT NULL
	DROP FUNCTION [dbo].udfEarnedInterest
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION udfEarnedInterest(
	@DateStart DATETIME,
	@DateEnd DATETIME
)
RETURNS @earned_interest TABLE (
	LoanID INT NOT NULL,
	EarnedInterest DECIMAL(18, 4) NOT NULL
)
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	DECLARE @loans LoanIdListTable

	INSERT INTO @loans
	SELECT DISTINCT
		Id
	FROM
		Loan
	WHERE
		@DateStart <= Date AND Date < @DateEnd
	UNION
	SELECT DISTINCT
		LoanId
	FROM
		LoanSchedule
	WHERE
		@DateStart <= Date AND Date < @DateEnd

	INSERT INTO @earned_interest
	SELECT
		LoanID,
		EarnedInterest
	FROM
		dbo.udfEarnedInterestForLoans(@DateStart, @DateEnd, @loans)

	RETURN
END
GO
