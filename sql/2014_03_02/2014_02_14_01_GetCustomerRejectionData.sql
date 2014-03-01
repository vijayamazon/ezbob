IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerRejectionData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomerRejectionData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomerRejectionData] 
	(@CustomerId INT,
	 @Reject_Defaults_Months INT,
	 @Reject_Defaults_Amount INT)
AS
BEGIN
	DECLARE @HasAccountingAccounts BIT,
			@ErrorMPsNum INT,
			@Counter INT,
			@ApprovalNum INT,
			@NumOfDefaultAccounts INT, 
			@MarketPlaceGroup INT, 
			@BankGroup INT, 
			@RowNum INT
	
	IF EXISTS (SELECT 1 FROM MP_CustomerMarketPlace, MP_MarketplaceType WHERE MP_CustomerMarketPlace.CustomerId = @CustomerId AND MP_CustomerMarketPlace.MarketPlaceId = MP_MarketplaceType.Id AND (MP_MarketplaceType.GroupId = @MarketPlaceGroup OR MP_MarketplaceType.GroupId = @BankGroup) AND MP_MarketplaceType.Name != 'Pay Pal')
	BEGIN
		SET @HasAccountingAccounts = 1
	END
	ELSE
	BEGIN
		SET @HasAccountingAccounts = 0
	END
	
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
 
 
	
	SELECT @HasAccountingAccounts AS HasAccountingAccounts, 
		@ErrorMPsNum AS ErrorMPsNum, 
		@ApprovalNum AS ApprovalNum, 
		@NumOfDefaultAccounts AS NumOfDefaultAccounts
END
GO
