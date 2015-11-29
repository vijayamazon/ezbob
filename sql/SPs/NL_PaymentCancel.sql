IF OBJECT_ID('NL_PaymentCancel') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaymentCancel AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_PaymentCancel]
	@Tbl NL_PaymentsList READONLY,	
	@PaymentID BIGINT				
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @PaymentStatusID INT = (SELECT PaymentStatusID FROM @Tbl);

	IF @PaymentStatusID NOT IN (SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE [PaymentStatus] IN ('WrongPayment', 'ChargeBack'))
		RETURN -1; 

	--UPDATABLE: [PaymentStatusID], [DeletionTime], [DeletionNotificationTime], [DeletedByUserID], [Notes]

	UPDATE [dbo].[NL_Payments]  
	SET 
		[PaymentStatusID]= @PaymentStatusID,
		[DeletionTime] = (SELECT DeletionTime FROM @Tbl), 
		--[DeletionNotificationTime] = (SELECT DeletionNotificationTime FROM @Tbl ), 	
		[DeletedByUserID] = (SELECT [DeletedByUserID] FROM @Tbl),		
		[Notes] = concat([Notes], '\n' , (SELECT [Notes]  FROM @Tbl))
	WHERE 
		[PaymentID] = @PaymentID;
		
	-- reset paid amounts
	DECLARE @LoanID bigint;
	DECLARE @PaymentDate datetime;
	SET  @LoanID = (SELECT LoanID FROM @Tbl);
	SET @PaymentDate = (SELECT PaymentTime from [dbo].[NL_Payments] WHERE [PaymentID] = @PaymentID);

	EXEC [NL_ResetPaidAmountsAndStatuses] @PaymentDate = @PaymentDate, @LoanID = @LoanID, @PaymentDateInclude = 1;
	
	-- return PaymentID
	SELECT 0;
		
END