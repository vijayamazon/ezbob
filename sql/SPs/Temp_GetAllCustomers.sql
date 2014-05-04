IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Temp_GetAllCustomersWithCompany]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Temp_GetAllCustomersWithCompany]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Temp_GetAllCustomersWithCompany] 
AS
BEGIN
	SELECT 
		Customer.Id,
		Company.ExperianRefNum
	FROM 
		Customer,
		Company
	WHERE
		Customer.CompanyId = Company.Id AND
		Company.ExperianRefNum IS NOT NULL AND
		(
			Company.TypeOfBusiness = 'Limited' OR 
			Company.TypeOfBusiness = 'LLP'
		)
END
GO
