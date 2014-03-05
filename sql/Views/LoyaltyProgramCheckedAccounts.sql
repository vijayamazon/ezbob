IF OBJECT_ID (N'dbo.LoyaltyProgramCheckedAccounts') IS NOT NULL
	DROP VIEW dbo.LoyaltyProgramCheckedAccounts
GO

CREATE VIEW [dbo].[LoyaltyProgramCheckedAccounts]
AS
SELECT
	clp.CustomerMarketPlaceID
FROM
	CustomerLoyaltyProgram clp
	INNER JOIN LoyaltyProgramActions a ON clp.ActionID = a.ActionID
WHERE
	a.ActionName = 'ACCOUNTCHECKED'

GO

