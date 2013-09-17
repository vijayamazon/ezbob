IF OBJECT_ID('RptLoanStats_CustomerPostcodes') IS NOT NULL
	DROP PROCEDURE RptLoanStats_CustomerPostcodes
GO

CREATE PROCEDURE RptLoanStats_CustomerPostcodes
AS
SELECT
	c.Id AS CustomerID,
	a.Rawpostcode
FROM
	Customer c
	INNER JOIN CustomerAddress a
		ON c.Id = a.CustomerId
		AND a.addressType = 1
WHERE
	c.IsTest = 0
GO
