IF OBJECT_ID('LoadCustomerInfo') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerInfo AS SELECT 1')
GO

ALTER PROCEDURE LoadCustomerInfo
@CustomerID INT
AS
	SELECT
		c.Name AS Email,
		c.FirstName AS FirstName,
		c.Surname AS LastName
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID
GO
