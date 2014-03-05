IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCompanyData] 
	(@CustomerId INT)
AS
BEGIN
	SELECT
		Company.TypeOfBusiness AS CompanyType,		
		Company.ExperianRefNum
	FROM
		Customer,
		Company
	WHERE
		Customer.Id = @CustomerId AND
		Customer.CompanyId = Company.Id
END
GO
