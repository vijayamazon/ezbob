IF OBJECT_ID('NL_LoanRolloverUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanRolloverUpdate AS SELECT 1')
GO


ALTER PROCEDURE dbo.NL_LoanRolloverUpdate
@Tbl NL_LoanRolloversList READONLY,
@RolloverID bigint
AS
BEGIN
	SET NOCOUNT ON;

	if @RolloverID  = 0 
		return;

	update dbo.NL_LoanRollovers set
		LoanHistoryID = ISNULL((select LoanHistoryID from @Tbl), LoanHistoryID ),
		CreatedByUserID= ISNULL((select CreatedByUserID from @Tbl), CreatedByUserID ),
		DeletedByUserID= ISNULL((select DeletedByUserID from @Tbl), DeletedByUserID ),
		LoanFeeID= ISNULL((select LoanFeeID from @Tbl),LoanFeeID ),
		CreationTime= ISNULL((select CreationTime from @Tbl),CreationTime ),
		ExpirationTime= ISNULL((select ExpirationTime from @Tbl),ExpirationTime ),
		CustomerActionTime = ISNULL((select CustomerActionTime from @Tbl),CustomerActionTime ),
		IsAccepted= ISNULL((select IsAccepted from @Tbl),IsAccepted ),
		DeletionTime= ISNULL((select DeletionTime from @Tbl),DeletionTime )
	where
		LoanRolloverID = @RolloverID;


END