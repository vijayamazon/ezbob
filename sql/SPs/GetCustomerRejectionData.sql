IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerRejectionData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerRejectionData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerRejectionData] 
	(@CustomerId INT,
	 @Reject_Defaults_Months INT,
	 @Reject_Defaults_Amount INT,
	 @RejectByCompany_Defaults_Months INT,
	 @RejectByCompany_Defaults_Amount INT,
	 @Reject_Late_Last_Months INT)
AS
BEGIN
	DECLARE @ErrorMPsNum INT,
			@Counter INT,
			@ApprovalNum INT,
			@NumOfDefaultAccounts INT,
			@NumOfDefaultAccountsForCompany INT, 
			@MarketPlaceGroup INT, 
			@BankGroup INT, 
			@RowNum INT
		
	SELECT 
		@ErrorMPsNum = COUNT(cmp.Id)
	FROM 
		MP_CustomerMarketPlace cmp
	WHERE 
		cmp.UpdateError IS NOT NULL AND 
		cmp.UpdateError != '' AND 
		cmp.UpdatingEnd IS NOT NULL AND 
		CustomerId = @CustomerId		
		
	SELECT 
		@Counter = COUNT(1)
	FROM 
		LoanSchedule
	WHERE 
		Status='Late' AND
		LoanId IN
		(
		SELECT 
			Id 
		FROM 
			Loan 
		WHERE 
			RequestCashId = 
				(
				SELECT 
					Id 
				FROM 
					(
						SELECT 
							ROW_NUMBER() OVER (ORDER BY Id DESC) AS row, 
							cr.Id 
						FROM 
							CashRequests cr
						WHERE 
							cr.IdCustomer = @CustomerId
					) p
				WHERE 
					p.row=@RowNum
				)
		) 
	
	
	
	IF @Counter = 0
	BEGIN
		SELECT 
			@ApprovalNum = COUNT(cr.id) 
		FROM 
			CashRequests cr 
		WHERE 
			cr.IdCustomer = @CustomerId AND 
			cr.UnderwriterDecision= 'Approved'
	END
	ELSE 
	BEGIN
		SELECT @ApprovalNum = NULL
	END
 
	SELECT @NumOfDefaultAccounts = NumOfDefaultAccounts FROM [GetNumOfDefaultAccounts] (@CustomerId, @Reject_Defaults_Months, @Reject_Defaults_Amount)
 	SELECT @NumOfDefaultAccountsForCompany = NumOfDefaultAccounts FROM [GetNumOfDefaultAccounts] (@CustomerId, @RejectByCompany_Defaults_Months, @RejectByCompany_Defaults_Amount)
 	
 	
 	DECLARE @NumOfLateAccounts = (SELECT ISNULL(SUM(y.late), 0) NumOfLateAccounts
								  FROM 
								  (
										SELECT CASE WHEN x.s LIKE '%2%' THEN 1
												    WHEN x.s LIKE '%3%' THEN 1
												    WHEN x.s LIKE '%4%' THEN 1
												    WHEN x.s LIKE '%5%' THEN 1
												    WHEN x.s LIKE '%6%' THEN 1
												    WHEN x.s LIKE '%7%' THEN 1
												    WHEN x.s LIKE '%8%' THEN 1
												    WHEN x.s LIKE '%9%' THEN 1
												    ELSE 0 END AS late
										FROM
										(
											SELECT RIGHT(RTRIM(Status12Months), @Reject_Late_Last_Months) AS s FROM ExperianDL97Accounts 
											WHERE CustomerId=@CustomerId
											AND State = 'A'
										)x
								  ) y
								 )
	SELECT 
		@ErrorMPsNum AS ErrorMPsNum, 
		@ApprovalNum AS ApprovalNum, 
		@NumOfDefaultAccounts AS NumOfDefaultAccounts,
		@NumOfDefaultAccountsForCompany AS NumOfDefaultAccountsForCompany,
		@NumOfLateAccounts AS NumOfLateAccounts
END
GO
