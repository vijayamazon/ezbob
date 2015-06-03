IF OBJECT_ID('GetCompanyName') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyName AS SELECT 1')
GO

ALTER PROCEDURE GetCompanyName
	(@CustomerId int)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
	co.Id CompanyId,
	co.ExperianCompanyName CompanyName
	FROM
	Customer c
	LEFT JOIN
	Company co
	ON
	c.CompanyId=co.Id
	WHERE
	c.id=@CustomerID
END
GO
