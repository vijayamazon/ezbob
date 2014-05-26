IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalInfoForExperianCompanyCheck]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalInfoForExperianCompanyCheck]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalInfoForExperianCompanyCheck] 
	(@CustomerId INT)
AS
BEGIN		
	SELECT 
		Company.ExperianRefNum, 
		Company.ExperianCompanyName,
		Company.TypeOfBusiness
	FROM 
		Company,
		Customer
	WHERE
		Company.Id = Customer.CompanyId AND
		Customer.Id = @CustomerId		
END
GO
