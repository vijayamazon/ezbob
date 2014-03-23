IF OBJECT_ID('GetLastStepCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastStepCustomers AS SELECT 1')
GO

ALTER PROCEDURE GetLastStepCustomers
@DateStart DATETIME,
@DateEnd DATETIME,
@IncludeTest BIT = 0
AS
BEGIN
	SELECT DISTINCT
		c.Name AS eMail,
		c.FirstName AS FirstName,
		c.Surname AS SurName,
		CASE
			WHEN cr.ManagerApprovedSum IS NOT NULL
				THEN cr.ManagerApprovedSum
			ELSE cr.SystemCalculatedSum
		END AS MaxApproved
	FROM
		CashRequests cr
		INNER JOIN Customer c ON cr.IdCustomer = c.Id AND (@IncludeTest = 0 OR c.IsTest = 0)
		LEFT JOIN (
			SELECT DISTINCT
				l.CustomerId
			FROM
				Loan l
				INNER JOIN Customer c ON l.CustomerId = c.Id AND (@IncludeTest = 0 OR c.IsTest = 0)
			WHERE
				@DateStart <= CONVERT(DATE, l.Date)
		) lt ON c.Id = lt.CustomerId
	WHERE
		lt.CustomerId IS NULL
		AND (
			(cr.IdUnderwriter IS NOT NULL AND cr.UnderwriterDecision = 'Approved')
			OR
			(cr.IdUnderwriter IS NULL AND cr.SystemDecision = 'Approve')
		)
		AND (
			(cr.IdUnderwriter IS NOT NULL AND CONVERT(DATE, cr.UnderwriterDecisionDate) = @DateStart)
			OR
			(cr.IdUnderwriter IS NULL AND CONVERT(DATE, cr.SystemDecisionDate) = @DateStart)
		)
END
GO
