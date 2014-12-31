IF OBJECT_ID('LoadCustomerCompanyDetails') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerCompanyDetails AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerCompanyDetails
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CompanyID = co.Id,
		TypeOfBusinessStr = co.TypeOfBusiness,
		co.ExperianRefNum
	FROM
		Company co
		INNER JOIN Customer c
			ON co.Id = c.CompanyId
			AND c.Id = @CustomerID
END
GO	
