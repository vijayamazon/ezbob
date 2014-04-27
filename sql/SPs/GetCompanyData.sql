IF OBJECT_ID('GetCompanyData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanyData 
@CustomerId INT
AS
BEGIN
	SELECT
		Company.TypeOfBusiness AS CompanyType,		
		Company.ExperianRefNum
	FROM
		Customer
		INNER JOIN Company ON Customer.CompanyId = Company.Id
	WHERE
		Customer.Id = @CustomerId
END
GO
