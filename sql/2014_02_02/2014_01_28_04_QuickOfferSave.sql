IF OBJECT_ID('QuickOfferSave') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferSave AS SELECT 1')
GO

ALTER PROCEDURE QuickOfferSave
@CustomerID INT,
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
@QuickOfferID INT OUTPUT
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
