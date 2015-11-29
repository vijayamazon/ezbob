SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaymentsSave') IS NULL
	EXECUTE('CREATE PROCEDURE NL_PaymentsSave AS SELECT 1')
GO

ALTER PROCEDURE NL_PaymentsSave
@Tbl NL_PaymentsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_Payments (
		[PaymentMethodID],		
		[PaymentTime],
		[Amount],
		[PaymentStatusID],
		[CreationTime],
		[CreatedByUserID],
		[DeletionTime],
		[DeletedByUserID],
		[Notes],
		[LoanID]
	) SELECT
		[PaymentMethodID],	
		[PaymentTime],
		[Amount],
		[PaymentStatusID],
		[CreationTime],
		[CreatedByUserID],
		[DeletionTime],
		[DeletedByUserID],
		[Notes],
		[LoanID]
	FROM @Tbl;
	
	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	DECLARE @LoanID bigint 
	
	DECLARE @PaymentTime datetime ;
	SET @PaymentTime =  (select [PaymentTime] from @Tbl);
	
	--  inserted paymentTime is retroactive
	if (select max(PaymentTime) from NL_Payments) <> @PaymentTime
	BEGIN
		SET  @LoanID= (SELECT LoanID FROM @Tbl);
		EXEC [NL_ResetPaidAmountsAndStatuses]  @PaymentDate = @PaymentTime, @LoanID = @LoanID;
	END
		
	SELECT @ScopeID AS ScopeID
END
GO


