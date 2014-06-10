IF OBJECT_ID('GetPersonalInfoForExperianCompanyCheck') IS NULL
	EXECUTE('CREATE PROCEDURE GetPersonalInfoForExperianCompanyCheck AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetPersonalInfoForExperianCompanyCheck
@CustomerId INT
AS
BEGIN
	SELECT 
		MP_ExperianDataCache.JsonPacket AS CompanyData,
		Company.ExperianRefNum, 
		Company.ExperianCompanyName,
		Company.TypeOfBusiness
	FROM 
		MP_ExperianDataCache
		INNER JOIN Company ON MP_ExperianDataCache.CompanyRefNumber = Company.ExperianRefNum
		INNER JOIN Customer ON Company.Id = Customer.CompanyId
	WHERE 
		Customer.Id = @CustomerId
END
GO
