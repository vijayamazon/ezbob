SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadLogicalGlueLessCustomers') IS NULL
	EXECUTE('CREATE PROCEDURE LoadLogicalGlueLessCustomers AS SELECT 1')
GO

ALTER PROCEDURE LoadLogicalGlueLessCustomers
AS
BEGIN
	DECLARE @Now DATETIME = GETUTCDATE()

	;WITH lg_compliant AS (
		SELECT DISTINCT
			IsRegulated
		FROM
			I_ProductSubType
		WHERE
			AutoDecisionInternalLogic = 0
	), crl AS (
		SELECT
			CustomerID,
			EntryID = MAX(Id)
		FROM
			CustomerRequestedLoan
		GROUP BY
			CustomerID
	) SELECT
		CustomerID = c.Id,
		c.OriginID,
		rl.Amount,
		rl.Term
	FROM
		Customer c
		INNER JOIN Company co ON c.CompanyId = co.Id
		INNER JOIN TypeOfBusiness t ON co.TypeOfBusiness = t.Name
		INNER JOIN lg_compliant lgc ON t.IsRegulated = lgc.IsRegulated
		LEFT JOIN CustomerLogicalGlueHistory h
			ON c.Id = h.CustomerID
			AND co.Id = h.CompanyID
			AND h.IsActive = 1
			AND (h.GradeID IS NOT NULL OR h.IsHardReject = 1)
			AND DATEADD(day, 30, h.SetTime) < @Now
		LEFT JOIN crl ON c.Id = crl.CustomerID
		LEFT JOIN CustomerRequestedLoan rl ON crl.EntryID = rl.Id
	WHERE
		c.IsTest = 0
	ORDER BY
		c.Id
END
GO
