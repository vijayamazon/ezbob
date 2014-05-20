IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetAllCustomersWithCompany]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetAllCustomersWithCompany]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAllCustomersWithCompany] 
AS
BEGIN
	SELECT 
		Customer.Id AS CustomerId,
		Company.ExperianRefNum AS RefNumber,
		Company.TypeOfBusiness,
		MP_ExperianDataCache.JsonPacket AS Response		
	FROM 
		Customer,
		Company,
		MP_ExperianDataCache
	WHERE
		Customer.CompanyId = Company.Id AND
		Company.ExperianRefNum IS NOT NULL AND
		ExperianRefNum != 'NotFound' AND
		ExperianRefNum != 'skip' AND
		ExperianRefNum != 'exception' AND
		MP_ExperianDataCache.CompanyRefNumber = Company.ExperianRefNum
END
GO
