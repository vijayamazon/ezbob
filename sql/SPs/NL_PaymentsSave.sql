SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_PaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_PaymentsSave
GO

IF TYPE_ID('NL_PaymentsList') IS NOT NULL
	DROP TYPE NL_PaymentsList
GO

CREATE TYPE NL_PaymentsList AS TABLE (		
		PaymentMethodID INT NOT NULL,
		PaymentTime DATETIME NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		PaymentStatusID INT NOT NULL,
		CreationTime DATETIME NOT NULL,
		CreatedByUserID INT NOT NULL,
		DeletionTime DATETIME NULL,		
		DeletedByUserID INT NULL,
		Notes NVARCHAR(MAX) NULL,
		PaymentDestination NVARCHAR(20) NULL,
		LoanID BIGINT NOT NULL
)
GO

CREATE PROCEDURE NL_PaymentsSave
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
		PaymentDestination,
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
		PaymentDestination,
		[LoanID]
	FROM @Tbl;
	
	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	DECLARE @LoanID bigint 
	
	DECLARE @PaymentTime datetime ;
	SET @PaymentTime =  (select [PaymentTime] from @Tbl);
	
	--  inserted paymentTime is retroactive
	if (select max(PaymentTime) from NL_Payments) > @PaymentTime
	BEGIN
		SET  @LoanID= (SELECT LoanID FROM @Tbl);
		EXEC [NL_ResetPaidAmountsAndStatuses]  @PaymentDate = @PaymentTime, @LoanID = @LoanID;
	END
		
	SELECT @ScopeID AS ScopeID
END
GO


