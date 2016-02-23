IF OBJECT_ID('NL_LoanHistoryUpdate')IS NULL 
	EXECUTE('CREATE PROCEDURE NL_LoanHistoryUpdate AS SELECT 1')
GO



ALTER PROCEDURE [dbo].[NL_LoanHistoryUpdate]
	@LoanHistoryID bigint,
	@LateFees decimal (18,6)=null,
	@DistributedFees decimal (18,6)=null,
	@OutstandingInterest decimal (18,6)=null
AS
BEGIN
	
	SET NOCOUNT ON;

	update [dbo].[NL_LoanHistory] set 
		[LateFees]=ISNULL(@LateFees,[LateFees]), 
		[DistributedFees]=ISNULL(@DistributedFees, [DistributedFees]),
		OutstandingInterest=ISNULL(@OutstandingInterest, OutstandingInterest)
	where [LoanHistoryID]=@LoanHistoryID;

END
