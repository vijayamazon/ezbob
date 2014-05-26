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
		MP_ExperianDataCache.JsonPacket AS CompanyData,
		Company.ExperianRefNum, 
		Company.ExperianCompanyName,
		Company.TypeOfBusiness
	FROM 
		MP_ExperianDataCache, 
		Company,
		Customer
	WHERE 
		MP_ExperianDataCache.CompanyRefNumber = Company.ExperianRefNum AND 
		Company.Id = Customer.CompanyId AND
		Customer.Id = @CustomerId		
END
GO
