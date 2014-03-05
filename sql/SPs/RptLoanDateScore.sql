IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanDateScore]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanDateScore]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLoanDateScore]
AS
BEGIN
	SELECT
		c.Id AS CustomerID,
		MAX(l.Date) AS LoanIssueDate,
		co.ExperianRefNum AS LimitedRefNum,
		co.ExperianRefNum AS NonLimitedRefNum
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		LEFT JOIN Company co ON co.Id = c.CompanyId
	GROUP BY
		c.Id,
		co.ExperianRefNum
	ORDER BY
		c.Id
END
GO
