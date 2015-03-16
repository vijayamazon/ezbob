IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyRefNumbers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyRefNumbers]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyData]') AND type in (N'P', N'PC'))
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
		TypeOfBusiness AS CompanyType,		
		ExperianRefNum
	FROM
		Company
	WHERE
		CustomerId = @CustomerId
END
GO


