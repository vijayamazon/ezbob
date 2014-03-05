IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanStats_CashRequests]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanStats_CashRequests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLoanStats_CashRequests]
AS
BEGIN
	SELECT
	r.Id AS RequestID,
	r.LoanTypeId AS LoanTypeID,
	lt.Type AS LoanType,
	r.IsLoanTypeSelectionAllowed,
	CASE dp.IsDefault
		WHEN 1 THEN '0'
		ELSE dp.Name
	END AS DiscountPlanName,
	c.IsOffline,
	c.Id AS CustomerID,
	c.Fullname AS CustomerName,
	CASE
		WHEN r.IdUnderwriter IS NULL THEN r.SystemDecisionDate
		ELSE r.UnderwriterDecisionDate
	END AS DecisionDate,
	ISNULL(CASE
		WHEN r.IdUnderwriter IS NULL
			THEN CASE
				WHEN r.UnderwriterComment = 'Auto Re-Approval' THEN r.ManagerApprovedSum
				ELSE r.SystemCalculatedSum
			END
		ELSE
			ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum)
	END, 0) AS ApprovedSum,
	r.InterestRate AS ApprovedRate,
	ISNULL(r.ExpirianRating, 0) AS CreditScore,
	ISNULL(r.AnualTurnover, 0) AS AnnualTurnover,
	r.MedalType,
	c.Gender,
	c.DateOfBirth,
	c.MaritalStatus,
	c.ResidentialStatus,
	c.TypeOfBusiness,
	c.ReferenceSource,
	ISNULL(lmt.LoanId, 0) AS LoanID,
	ISNULL(lmt.LoanAmount, 0) AS LoanAmount,
	ISNULL(lmt.Date, 'Jul 1 1976') AS LoanIssueDate,
	ISNULL(lmt.AgreementModel, '{ "Term": 0 }') AS AgreementModel
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN LoanType lt ON r.LoanTypeId = lt.Id
	INNER JOIN DiscountPlan dp ON r.DiscountPlanId = dp.Id
	LEFT JOIN (
		SELECT
			l.Id AS LoanId,
			l.RequestCashId,
			ISNULL(ISNULL(mt.Amount, l.LoanAmount), 0) AS LoanAmount,
			l.Date,
			l.AgreementModel
		FROM
			Loan l
			INNER JOIN LoanTransaction mt
				ON l.Id = mt.LoanId
				AND mt.Type = 'PacnetTransaction'
				AND mt.Status = 'Done'
				AND mt.Description NOT LIKE 'Non-cash.%'
	) lmt ON r.Id = lmt.RequestCashId
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
		WHEN r.IdUnderwriter IS NULL THEN r.SystemDecisionDate
		ELSE r.UnderwriterDecisionDate
	END
END
GO
