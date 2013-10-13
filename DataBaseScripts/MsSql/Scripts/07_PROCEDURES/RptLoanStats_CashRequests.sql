IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanStats_CashRequests]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanStats_CashRequests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptLoanStats_CashRequests
AS
SELECT
	r.Id AS RequestID,
	r.LoanTypeId AS LoanTypeID,
	lt.Type AS LoanType,
	r.IsLoanTypeSelectionAllowed,
	CASE dp.IsDefault
		WHEN 1 THEN '0'
		ELSE dp.Name
	END AS DiscountPlanName,
	c.Id AS CustomerID,
	c.Fullname AS CustomerName,
	CASE
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END AS DecisionDate,
	ISNULL(CASE
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemCalculatedSum
		ELSE
			ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum)
	END, 0) AS ApprovedSum,
	r.InterestRate AS ApprovedRate,
	ISNULL(r.ExpirianRating, 0) AS CreditScore,
	ISNULL(r.AnualTurnover, 0) AS AnnualTurnover,
	r.MedalType,
	c.Gender,
	c.DateOfBirth,
	c.MartialStatus,
	c.ResidentialStatus,
	c.TypeOfBusiness,
	c.ReferenceSource,
	ISNULL(l.Id, 0) AS LoanID,
	ISNULL(
		CASE
			WHEN mt.Description LIKE 'Non-cash.%' THEN 0
			ELSE ISNULL(mt.Amount, l.LoanAmount)
		END,
		0
	) AS LoanAmount,
	ISNULL(l.Date, 'Jul 1 1976') AS LoanIssueDate,
	ISNULL(l.AgreementModel, '{ "Term": 0 }') AS AgreementModel
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN LoanType lt ON r.LoanTypeId = lt.Id
	INNER JOIN DiscountPlan dp ON r.DiscountPlanId = dp.Id
	LEFT JOIN Loan l ON r.Id = l.RequestCashId
	LEFT JOIN LoanTransaction mt
		ON l.Id = mt.LoanId
		AND mt.Type = 'PacnetTransaction'
		AND mt.Type = 'Done'
WHERE
	(
		(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Approved')
		OR
		(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
	)
	AND
	r.CreationDate >= 'Sep 4 2012'
	
ORDER BY
	r.IdCustomer,
	CASE 
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END
GO
