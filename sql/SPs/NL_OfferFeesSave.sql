SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OfferFeesSave') IS NOT NULL
	DROP PROCEDURE NL_OfferFeesSave
GO

IF TYPE_ID('NL_OfferFeesList') IS NOT NULL
	DROP TYPE NL_OfferFeesList
GO

CREATE TYPE NL_OfferFeesList AS TABLE (	
	[LoanID] INT ,
	[OfferID] [INT]   ,	
	[LoanFeeTypeID] [INT]   ,
	[Percent] [DECIMAL](18, 6) ,
	[Amount] [DECIMAL](18, 6) 
)
GO

CREATE PROCEDURE NL_OfferFeesSave
@Tbl NL_OfferFeesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanFees (		
		[LoanID],
		[OfferID],	
		[LoanFeeTypeID],
		[Percent],
		[Amount]  
	) SELECT		
		[LoanID],
		[OfferID],	
		[LoanFeeTypeID],
		[Percent],
		[Amount] 
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


