IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyProgramCheckedAccounts]'))
DROP VIEW [dbo].[LoyaltyProgramCheckedAccounts]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW LoyaltyProgramCheckedAccounts
AS
SELECT
	clp.CustomerMarketPlaceID
FROM
	CustomerLoyaltyProgram clp
	INNER JOIN LoyaltyProgramActions a ON clp.ActionID = a.ActionID
WHERE
	a.ActionName = 'ACCOUNTCHECKED'
GO
