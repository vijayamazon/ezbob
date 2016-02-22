IF OBJECT_ID('NL_PaymentCancel') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaymentCancel AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_PaymentCancel]
	@PaymentID BIGINT,
	@LoanID BIGINT,
	@PaymentStatusID INT,
	@DeletionTime DATETIME,
	@DeletedByUserID INT,
	@Notes nvarchar(max) = null
AS
BEGIN
	SET NOCOUNT ON;	
			
	IF @PaymentStatusID NOT IN (SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE [PaymentStatus] IN ('WrongPayment', 'ChargeBack'))	
		RETURN -1000; 

	--UPDATABLE: [PaymentStatusID], [DeletionTime], [DeletedByUserID], [Notes]

	UPDATE [dbo].[NL_Payments]  
	SET 
		[PaymentStatusID]= @PaymentStatusID,
		[DeletionTime] = @DeletionTime, 
		[DeletedByUserID] = @DeletedByUserID,	
		[Notes] = ISNULL(@Notes, [Notes])
	WHERE 
		[PaymentID] = @PaymentID and LoanID = @LoanID;
		
	-- reset paid amounts
	
	DECLARE @PaymentDate datetime;

	SET @PaymentDate = (SELECT PaymentTime from [dbo].[NL_Payments] WHERE [PaymentID] = @PaymentID);
	
	EXEC [NL_ResetPaidAmountsAndStatuses] @PaymentDate = @PaymentDate, @LoanID = @LoanID, @PaymentDateInclude = 1;
	
	SELECT @PaymentID;
		
END