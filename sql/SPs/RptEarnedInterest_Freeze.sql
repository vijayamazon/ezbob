IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptEarnedInterest_Freeze]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptEarnedInterest_Freeze]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptEarnedInterest_Freeze]
AS
BEGIN
	SELECT
	f.LoanId,
	f.StartDate,
	f.EndDate,
	f.InterestRate,
	f.ActivationDate,
	f.DeactivationDate
FROM
	LoanInterestFreeze f
	INNER JOIN Loan l ON f.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
ORDER BY
	f.LoanId,
	(CASE WHEN f.DeactivationDate IS NULL THEN 1 ELSE 0 END),
	f.ActivationDate
END
GO
