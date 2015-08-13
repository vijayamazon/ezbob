IF OBJECT_ID('NL_OfferFeesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_OfferFeesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_OfferFeesGet
@OfferID BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		[OfferFeeID]  ,
		[OfferID]  ,	
		[LoanFeeTypeID] ,
		[Percent]  ,
		[AbsoluteAmount]  ,
		[OneTimePartPercent] ,
		[DistributedPartPercent]	
	FROM [dbo].[NL_OfferFees] WHERE	[OfferID]=@OfferID
END

GO


