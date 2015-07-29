SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OfferFeesSave') IS NOT NULL
	DROP PROCEDURE NL_OfferFeesSave
GO

IF TYPE_ID('NL_OfferFeesList') IS NOT NULL
	DROP TYPE NL_OfferFeesList
GO

 CREATE TYPE NL_OfferFeesList AS TABLE (		
	 [OfferID] INT NOT NULL ,	
	 [LoanFeeTypeID] INT NOT NULL ,
	 [Percent] DECIMAL(18, 6) NULL,
	 [Amount] DECIMAL(18, 6) NULL ,
	 [OneTimePartPercent] [DECIMAL](18, 6) NULL,
	 [DistributedPartPercent] [DECIMAL](18, 6) NULL	 
 )
 GO

CREATE PROCEDURE NL_OfferFeesSave
@Tbl NL_OfferFeesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	 INSERT INTO NL_OfferFees (		
		[OfferID]  ,	
		[LoanFeeTypeID] ,
		[Percent]  ,
		[Amount]  ,
		[OneTimePartPercent] ,
		[DistributedPartPercent]		 
	 ) SELECT			
		[OfferID]  ,	
		[LoanFeeTypeID] ,
		[Percent]  ,
		[Amount]  ,
		[OneTimePartPercent] ,
		[DistributedPartPercent]
	 FROM @Tbl

	 DECLARE @ScopeID INT = SCOPE_IDENTITY()
	 SELECT @ScopeID AS ScopeID
END
GO