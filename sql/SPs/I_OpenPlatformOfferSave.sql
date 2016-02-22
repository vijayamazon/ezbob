SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_OpenPlatformOfferSave') IS NOT NULL
	DROP PROCEDURE I_OpenPlatformOfferSave
GO

IF TYPE_ID('I_OpenPlatformOfferList') IS NOT NULL
	DROP TYPE I_OpenPlatformOfferList
GO

CREATE TYPE I_OpenPlatformOfferList AS TABLE (
	[InvestorID] INT NOT NULL,
	[CashRequestID] BIGINT NOT NULL,
	[InvestmentPercent] DECIMAL(18, 6) NOT NULL,
	[NLOfferID] BIGINT NULL
)
GO

CREATE PROCEDURE I_OpenPlatformOfferSave
@Tbl I_OpenPlatformOfferList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_OpenPlatformOffer (
		[InvestorID],
		[CashRequestID],
		[InvestmentPercent],
		[NLOfferID]
	) SELECT
		[InvestorID],
		[CashRequestID],
		[InvestmentPercent],
		[NLOfferID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


