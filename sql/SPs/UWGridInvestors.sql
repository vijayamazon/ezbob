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
		i.Timestamp
	FROM
		I_Investor i INNER JOIN I_InvestorType t ON t.InvestorTypeID = i.InvestorTypeID
	WHERE 
		i.IsActive=1	
	ORDER BY
		i.InvestorID DESC
END

GO


