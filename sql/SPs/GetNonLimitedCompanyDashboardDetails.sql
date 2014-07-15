IF OBJECT_ID('GetNonLimitedCompanyDashboardDetails') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCompanyDashboardDetails AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCompanyDashboardDetails
	(@CustomerId INT,		
	 @RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		BusinessName,
		RiskScore,
		AgeOfMostRecentJudgmentDuringOwnershipMonths,
		TotalJudgmentCountLast24Months,
		TotalAssociatedJudgmentCountLast24Months
	FROM 
		ExperianNonLimitedResults 
	WHERE 
		CustomerId = @CustomerId AND 
		RefNumber = @RefNumber AND 
		IsActive = 1
END
GO
