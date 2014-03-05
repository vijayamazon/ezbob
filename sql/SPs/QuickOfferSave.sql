IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QuickOfferSave]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[QuickOfferSave]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[QuickOfferSave] 
	(@CustomerID INT,
@Amount DECIMAL(18, 2),
@Aml INT,
@BusinessScore INT,
@IncorporationDate DATETIME,
@TangibleEquity DECIMAL(18, 2),
@TotalCurrentAssets DECIMAL(18, 2),
@ImmediateTerm INT,
@ImmediateInterestRate DECIMAL(18, 6),
@ImmediateSetupFee DECIMAL(18, 6),
@PotentialAmount DECIMAL(18, 6),
@PotentialTerm INT,
@PotentialInterestRate DECIMAL(18, 6),
@PotentialSetupFee DECIMAL(18, 6),
@QuickOfferID INT OUTPUT)
AS
BEGIN
	INSERT INTO QuickOffer(
		Amount, CreationDate, ExpirationDate, Aml, BusinessScore,
		IncorporationDate, TangibleEquity, TotalCurrentAssets,
		ImmediateTerm, ImmediateInterestRate, ImmediateSetupFee,
		PotentialAmount, PotentialTerm, PotentialInterestRate,PotentialSetupFee
	) VALUES (
		@Amount, GETUTCDATE(), DATEADD(hour, dbo.udfQuickOfferDuration(), GETUTCDATE()), @Aml, @BusinessScore,
		@IncorporationDate, @TangibleEquity, @TotalCurrentAssets,
		@ImmediateTerm, @ImmediateInterestRate, @ImmediateSetupFee,
		@PotentialAmount, @PotentialTerm, @PotentialInterestRate, @PotentialSetupFee
	)

	SET @QuickOfferID = SCOPE_IDENTITY()

	UPDATE Customer SET QuickOfferID = @QuickOfferID WHERE Id = @CustomerID
END
GO
