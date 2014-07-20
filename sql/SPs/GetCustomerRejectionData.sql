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
	 @Reject_Defaults_CompanyMonths INT,
	 @Reject_Defaults_CompanyAmount INT)
AS
BEGIN
	DECLARE @ErrorMPsNum INT,
			@Counter INT,
			@ApprovalNum INT,
			@NumOfDefaultAccounts INT,
			@MarketPlaceGroup INT, 
			@BankGroup INT, 
			@RowNum INT,
			@CompanyDefaultStartDate DATETIME,
			@NumOfDefaultCompanyAccounts INT

	DECLARE @ServiceLogID BIGINT

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
		
	SELECT @CompanyDefaultStartDate = DATEADD(MM, -@Reject_Defaults_CompanyMonths, GETUTCDATE())
	
	SET @ServiceLogID = dbo.udfGetCustomerCompanyLogID(@CustomerID)

	SELECT
		@NumOfDefaultCompanyAccounts = COUNT(1)
	FROM
		ExperianLtdDL97 dl97
		INNER JOIN ExperianLtd ltd ON dl97.ExperianLtdID = ltd.ExperianLtdID
		INNER JOIN MP_ServiceLog log ON log.Id = @ServiceLogID
	WHERE
		log.InsertDate > @CompanyDefaultStartDate
		AND
		dl97.CurrentBalance >= @Reject_Defaults_CompanyAmount
		AND
		dl97.AccountState = 'D'

	SELECT 
		@ErrorMPsNum AS ErrorMPsNum, 
		@ApprovalNum AS ApprovalNum, 
		@NumOfDefaultAccounts AS NumOfDefaultAccounts,
		@NumOfDefaultCompanyAccounts AS NumOfDefaultCompanyAccounts
END
GO
