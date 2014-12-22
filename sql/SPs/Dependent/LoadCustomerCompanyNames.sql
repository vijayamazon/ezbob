IF OBJECT_ID('LoadCustomerCompanyNames') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerCompanyNames AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerCompanyNames
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	EXECUTE LoadHmrcBusinessNames @CustomerID, @Now, 1

	SELECT
		RowType             = 'CompanyName',
		ExperianCompanyName = co.ExperianCompanyName,
		EnteredCompanyName  = co.CompanyName
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerID
END
GO
