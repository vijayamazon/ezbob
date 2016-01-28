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
	 funding_account AS (
		 SELECT 
			 iba.InvestorBankAccountID AS InvestorBankAccountID,
			 iba.InvestorID AS InvestorID,
			 iba.BankAccountNumber AS BankAccountNumber     
		 FROM 
		 	I_InvestorBankAccount iba
		 WHERE 
		 	iba.IsActive=1 
		 AND 
		 	iba.InvestorAccountTypeID=@FundsAccountTypeID	   		
     ),
     repayment_account AS (
		 SELECT 
			 iba.InvestorBankAccountID AS InvestorBankAccountID,
			 iba.InvestorID AS InvestorID,
			 iba.BankAccountNumber AS BankAccountNumber   
		 FROM 
		 	 I_InvestorBankAccount iba
		 WHERE 
		 	iba.IsActive=1
		 AND 
		 	iba.InvestorAccountTypeID=@RepaymentsAccountTypeID
     ),     
	 last_funding_bat_newbalances AS(
		 SELECT 
		 	MAX(ibat.InvestorBankAccountTransactionID) AS maxid, fa.InvestorID
		 FROM 
		 	funding_account fa 
		 LEFT JOIN 
		 	I_InvestorBankAccountTransaction ibat ON fa.InvestorBankAccountID = ibat.InvestorBankAccountID
		 GROUP BY 
		 	fa.InvestorID
	 ),
   	 last_repayments_sb_newbalances AS(
	 SELECT 
	 	MAX(isb.InvestorSystemBalanceID) AS maxid,ra.InvestorID
	 FROM 
	 	repayment_account ra 
	 LEFT JOIN 
	 	I_InvestorSystemBalance isb ON ra.InvestorBankAccountID = isb.InvestorBankAccountID
	 GROUP BY 
	 	ra.InvestorID
	 )
	
	SELECT 
		i.InvestorID AS InvestorID,
		it.Name AS InvestorType,
		i.Name AS InvestorName,
		isnull(ibat.NewBalance,0) AS OutstandingFunding,
		isnull(isb.NewBalance,0) AS AccumulatedRepayments,
		(SELECT 
			sum(sb.NewBalance) 
		 FROM 
		 	I_InvestorSystemBalance sb
		 INNER JOIN 
			I_InvestorBankAccount ba ON i.InvestorID = ba.InvestorID AND sb.InvestorBankAccountID = ba.InvestorBankAccountID
		WHERE 
			ba.InvestorAccountTypeID=@RepaymentsAccountTypeID 
		AND 
			ba.IsActive=0 
		AND
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
		fa.InvestorBankAccountID AS FundingBankAccountID,
		ra.InvestorBankAccountID AS RepaymentsBankAccountID,
		fa.BankAccountNumber AS FundingBankAccountNumber,
		ra.BankAccountNumber AS RepaymentsBankAccountNumber
	FROM
		I_Investor i
		LEFT JOIN 
			I_InvestorType it ON it.InvestorTypeID = i.InvestorTypeID
		LEFT JOIN	
			last_funding_bat_newbalances ibatf ON ibatf.InvestorID = i.InvestorID
		LEFT JOIN 
			last_repayments_sb_newbalances isbr ON isbr.InvestorID = i.InvestorID
		LEFT JOIN 
			funding_account fa ON fa.InvestorID = i.InvestorID
		LEFT JOIN 
			repayment_account ra ON ra.InvestorID = i.InvestorID
		LEFT JOIN 
			I_InvestorSystemBalance isb ON isb.InvestorSystemBalanceID = isbr.maxid
		LEFT JOIN 
			I_InvestorBankAccountTransaction ibat ON ibat.InvestorBankAccountTransactionID = ibatf.maxid	
	ORDER BY
		i.Name
END

GO