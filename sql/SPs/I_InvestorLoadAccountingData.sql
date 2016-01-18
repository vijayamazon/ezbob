IF OBJECT_ID('I_InvestorLoadAccountingData') IS NULL
	EXECUTE('CREATE PROCEDURE I_InvestorLoadAccountingData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_InvestorLoadAccountingData


AS
DECLARE @FundsAccountTypeID DECIMAL = (SELECT InvestorAccountTypeID FROM I_InvestorAccountType WHERE Name = 'Funding')
DECLARE @RepaymentsAccountTypeID DECIMAL = (SELECT InvestorAccountTypeID FROM I_InvestorAccountType WHERE Name = 'Repayments')

BEGIN
	SET NOCOUNT ON

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
     ),

   	 last_repayments_sb_newbalances AS(
	 SELECT 
	 MAX(InvestorSystemBalanceID) AS maxid
	 FROM 
	 I_InvestorSystemBalance 
	 GROUP BY InvestorBankAccountID
	 ),
     active_repayments_account_data AS (
	 SELECT 
	 isb.NewBalance AS NewBalance,
	 isb.InvestorBankAccountID AS InvestorBankAccountID,
	 iba.IsActive AS IsBankAccountActive,
	 iba.InvestorID AS InvestorID  
	 FROM 
	 I_InvestorSystemBalance isb
	 INNER JOIN last_repayments_sb_newbalances lisb ON lisb.maxid = isb.InvestorSystemBalanceID	 
	 LEFT JOIN I_InvestorBankAccount iba ON isb.InvestorBankAccountID = iba.InvestorBankAccountID
	 WHERE (iba.IsActive=1 OR (iba.IsActive=0 AND isb.NewBalance > 0)) AND iba.InvestorAccountTypeID=@RepaymentsAccountTypeID
     )

	SELECT 
		
	i.InvestorID AS InvestorID,
	it.Name AS InvestorType,
	i.Name AS InvestorName,
	ibatf.NewBalance AS OutstandingFunding,


	isbr.NewBalance AS AccumulatedRepayments,

	(SELECT sum(sb.NewBalance) FROM I_InvestorSystemBalance sb
		INNER JOIN I_InvestorBankAccount ba ON i.InvestorID = ba.InvestorID AND sb.InvestorBankAccountID = ba.InvestorBankAccountID
		WHERE ba.InvestorAccountTypeID=@RepaymentsAccountTypeID AND ba.IsActive=0 AND
		 	sb.InvestorSystemBalanceID =
			(	
				SELECT 
					max(sb1.InvestorSystemBalanceID)
				FROM 
					I_InvestorSystemBalance sb1
				WHERE 
					sb.InvestorBankAccountID = sb1.InvestorBankAccountID 
			)) AS TotalNonActiveAccumulatedRepayments,
		
	i.DiscountServicingFeePercent AS ServicingFeeDiscount,
	i.IsActive AS IsInvestorActive,		
	isbr.IsBankAccountActive AS IsRepaymentsBankAccountActive,
	ibatf.InvestorBankAccountID AS FundingBankAccountID,
	isbr.InvestorBankAccountID AS RepaymentsBankAccountID
	FROM
		I_Investor i
		LEFT JOIN I_InvestorType it ON it.InvestorTypeID = i.InvestorTypeID
		LEFT JOIN active_funding_account_data ibatf ON ibatf.InvestorID = i.InvestorID
		LEFT JOIN active_repayments_account_data isbr ON isbr.InvestorID = i.InvestorID

	ORDER BY
	i.Name
END
GO

