IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianDirectorsNamesForCustomer]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianDirectorsNamesForCustomer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianDirectorsNamesForCustomer]
	@CustomerId INT
AS
BEGIN
	SELECT 
		ExperianLtdDL72.FirstName, 
		ExperianLtdDL72.LastName 
	FROM 
		ExperianLtdDL72, 
		ExperianLtd, 
		Customer, 
		Company
	WHERE
		Customer.Id = @CustomerId AND
		Customer.CompanyId = Company.Id AND
		Company.ExperianRefNum = ExperianLtd.RegisteredNumber AND 
		ExperianLtdDL72.ExperianLtdID = ExperianLtd.ExperianLtdID
END
GO
