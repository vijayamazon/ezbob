IF OBJECT_ID('RptLoanStats_CashRequests') IS NOT NULL
	DROP PROCEDURE RptLoanStats_CashRequests
GO

CREATE PROCEDURE RptLoanStats_CashRequests
AS
SELECT
	r.Id AS RequestID,
	r.LoanTypeId AS LoanTypeID,
	lt.Type AS LoanType,
	r.IsLoanTypeSelectionAllowed,
	r.DiscountPlanId AS DiscountPlanID,
	dp.Name AS DiscountPlanName,
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
	ISNULL(l.Id, 0) AS LoanID,
	ISNULL(mt.Amount, l.LoanAmount) AS LoanAmount,
	l.Date AS LoanIssueDate,
	l.AgreementModel
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
	(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Approved')
	OR
	(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
ORDER BY
	r.IdCustomer,
	CASE 
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END
GO
