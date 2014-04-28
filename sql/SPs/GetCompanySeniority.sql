IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanySeniority]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanySeniority]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCompanySeniority] 
	(@CustomerId INT)
AS
BEGIN
	SELECT 
		MP_ExperianDataCache.JsonPacket AS CompanyData
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
