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
	 last_funding_bat_newbalances AS(
	 SELECT 
	 MAX(InvestorBankAccountTransactionID) AS maxid
	 FROM 
	 I_InvestorBankAccountTransaction 
	 GROUP BY InvestorBankAccountID
	 ),
     active_funding_account_data AS (
	 SELECT 
	 ibat.NewBalance AS NewBalance,
	 ibat.InvestorBankAccountID AS InvestorBankAccountID,
	 iba.InvestorID AS InvestorID    
	 FROM 
	 I_InvestorBankAccountTransaction ibat
	 INNER JOIN last_funding_bat_newbalances libat ON libat.maxid = ibat.InvestorBankAccountTransactionID
	 LEFT JOIN I_InvestorBankAccount iba ON ibat.InvestorBankAccountID = iba.InvestorBankAccountID
	 WHERE iba.IsActive=1 AND iba.InvestorAccountTypeID=@FundsAccountTypeID	   		
   )

	SELECT 
		i.InvestorID AS InvestorID,
		i.Name AS InvestorName, 
		sum(ibatf.NewBalance) AS InvestorFunds
	FROM 
		I_Investor i
		LEFT JOIN active_funding_account_data ibatf ON i.InvestorID = ibatf.InvestorID
		WHERE i.IsActive=1
	GROUP BY i.InvestorID, i.Name 
END
GO
