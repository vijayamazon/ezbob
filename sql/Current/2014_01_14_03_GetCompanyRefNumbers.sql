IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCompanyRefNumbers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCompanyRefNumbers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCompanyRefNumbers] 
	(@CustomerId INT)
AS
BEGIN	
	SELECT
		TypeOfBusiness AS CompanyType,
		LimitedRefNum,
		NonLimitedRefNum
	FROM
		Customer
	WHERE
		Id = @CustomerId
		
END
GO
