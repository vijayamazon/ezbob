IF OBJECT_ID('GetNonLimitedCompanyBasicDetails') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCompanyBasicDetails AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCompanyBasicDetails
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		Errors,
		RiskScore,
		CreditLimit,
		BusinessName,
		Address1,
		Address2,
		Address3,
		Address4,
		Address5,
		Postcode,
		IncorporationDate
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
