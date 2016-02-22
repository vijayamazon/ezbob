IF OBJECT_ID('UwGridInvestors') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridInvestors AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UwGridInvestors
@WithTest BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		i.InvestorID,
		t.Name AS InvestorType,
		i.Name CompanyName,
		i.FundingLimitForNotification, 
		i.Timestamp,
		i.IsActive,
		t.InvestorTypeID 

	FROM
		I_Investor i INNER JOIN I_InvestorType t ON t.InvestorTypeID = i.InvestorTypeID

	ORDER BY
		i.InvestorID 
END

GO

