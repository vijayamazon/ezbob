IF OBJECT_ID('NL_OfferFeesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_OfferFeesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_OfferFeesGet
@OfferID INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		[OfferFeeID]  ,
		[OfferID]  ,	
		[LoanFeeTypeID]  ,
		[Percent] ,
		[Amount] 	
	FROM [dbo].[NL_OfferFees] WHERE	[OfferID]=@OfferID
END

GO


