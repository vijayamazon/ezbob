SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OfferFeesSave') IS NOT NULL
	DROP PROCEDURE NL_OfferFeesSave
GO

IF TYPE_ID('NL_OfferFeesList') IS NOT NULL
	DROP TYPE NL_OfferFeesList
GO

CREATE TYPE NL_OfferFeesList AS TABLE (
	 [OfferID] BIGINT NOT NULL,
	 [LoanFeeTypeID] INT NOT NULL,
	 [Percent] DECIMAL(18, 6) NULL,
	 [AbsoluteAmount] DECIMAL(18, 6) NULL,
	 [OneTimePartPercent] DECIMAL(18, 6) NOT NULL,
	 [DistributedPartPercent] DECIMAL(18, 6) NOT NULL
)
GO

CREATE PROCEDURE NL_OfferFeesSave
@Tbl NL_OfferFeesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_OfferFees (
		[OfferID],
		[LoanFeeTypeID],
		[Percent],
		[AbsoluteAmount],
		[OneTimePartPercent],
		[DistributedPartPercent]
	) SELECT
		[OfferID],
		[LoanFeeTypeID],
		[Percent],
		[AbsoluteAmount],
		[OneTimePartPercent],
		[DistributedPartPercent]
	FROM
		@Tbl

	 DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	 SELECT @ScopeID AS ScopeID
END
GO
