IF OBJECT_ID('I_SystemBalanceAdd') IS NULL
	EXECUTE('CREATE PROCEDURE I_SystemBalanceAdd AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE I_SystemBalanceAdd
	@BankAccountID INT,
	@Date DATETIME,
	@TransactionAmount DECIMAL(18,6),
	@ServicingFeeAmount DECIMAL(18,6),
	@LoanTransactionID INT,
	@LoanID INT,
	@CashRequestID BIGINT,
	@Comment NVARCHAR(500),
	@UserID INT =NULL,
	@TransactionDate DATETIME=NULL,
	@NLOfferID BIGINT =NULL, 
	@NLLoanID BIGINT =NULL,
	@NLPaymentID BIGINT =NULL
AS
BEGIN
	SET NOCOUNT ON;


	DECLARE @PreviousBalance DECIMAL(18,6) = (SELECT 
												TOP 1 isnull(NewBalance, 0) 
											  FROM 
											  	I_InvestorSystemBalance 
											  WHERE 
											  	InvestorBankAccountID=@BankAccountID 
											  ORDER BY 
											  	Timestamp DESC)
	IF(@PreviousBalance IS NULL)
		SET @PreviousBalance = 0
	
	INSERT INTO I_InvestorSystemBalance (
		Timestamp,
		TransactionAmount,
		NewBalance,
		PreviousBalance,
		InvestorBankAccountID,
		ServicingFeeAmount,
		LoanTransactionID,
		LoanID, 
		CashRequestID,
		Comment,
		UserID,
		TransactionDate,
		NLOfferID , 
		NLLoanID ,
		NLPaymentID
	)
	VALUES (
		@Date, 
		@TransactionAmount, 
		@PreviousBalance + @TransactionAmount, 
		@PreviousBalance, 
		@BankAccountID, 
		@ServicingFeeAmount, 
		@LoanTransactionID,
		@LoanID,
		@CashRequestID,
		@Comment,
		@UserID,
		@TransactionDate,
		@NLOfferID, 
		@NLLoanID,
		@NLPaymentID 
	)
	
	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


