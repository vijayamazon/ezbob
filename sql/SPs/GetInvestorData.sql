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
	DECLARE @FundsAccountTypeID DECIMAL = (SELECT InvestorAccountTypeID FROM I_InvestorAccountType WHERE Name = 'Funding')

	;WITH
	 last_funding_sb_newbalances AS(
	 SELECT 
	 MAX(InvestorSystemBalanceID) AS maxid
	 FROM 
	 I_InvestorSystemBalance 
	 GROUP BY InvestorBankAccountID
	 ),
     active_funding_account_data AS (
	 SELECT 
	 isb.NewBalance AS NewBalance,
	 isb.InvestorBankAccountID AS InvestorBankAccountID,
	 iba.InvestorID AS InvestorID    
	 FROM 
	 I_InvestorSystemBalance isb
	 INNER JOIN last_funding_sb_newbalances lisb ON lisb.maxid = isb.InvestorSystemBalanceID
	 LEFT JOIN I_InvestorBankAccount iba ON isb.InvestorBankAccountID = iba.InvestorBankAccountID
	 WHERE iba.IsActive=1 AND iba.InvestorAccountTypeID=@FundsAccountTypeID	   		
   )

	SELECT 
		i.InvestorID AS InvestorID,
		i.Name AS InvestorName, 
		sum(isbf.NewBalance) AS InvestorFunds
	FROM 
		I_Investor i
		LEFT JOIN active_funding_account_data isbf ON i.InvestorID = isbf.InvestorID
		WHERE i.IsActive=1
	GROUP BY i.InvestorID, i.Name 
END
GO
