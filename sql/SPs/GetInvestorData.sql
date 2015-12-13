IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetInvestorData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].GetInvestorData
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetInvestorData]
AS
BEGIN
	DECLARE @FundsAccountID DECIMAL = (SELECT InvestorAccountTypeID FROM I_InvestorAccountType WHERE Name = 'Funding')

	SELECT 
		i.InvestorID AS InvestorID,
		i.Name AS InvestorName, 
		sum(sb.NewBalance) AS InvestorFunds
	FROM 
		I_Investor i
		LEFT JOIN I_InvestorBankAccount ba ON i.InvestorID = ba.InvestorID
		LEFT JOIN I_InvestorSystemBalance sb ON sb.InvestorBankAccountID = ba.InvestorBankAccountID
		WHERE i.IsActive=1 AND ba.InvestorAccountTypeID=@FundsAccountID AND ba.IsActive=1
	GROUP BY i.InvestorID, i.Name 
END
GO
