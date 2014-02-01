IF OBJECT_ID('QuickOffer') IS NULL
	CREATE TABLE QuickOffer (
		QuickOfferID INT IDENTITY(1, 1) NOT NULL,
		Amount DECIMAL(18, 2) NOT NULL,
		CreationDate DATETIME NOT NULL,
		ExpirationDate DATETIME NOT NULL,
		Aml INT NOT NULL,
		BusinessScore INT NOT NULL,
		IncorporationDate DATETIME NOT NULL,
		TangibleEquity DECIMAL(18, 2) NOT NULL,
		TotalCurrentAssets DECIMAL(18, 2) NOT NULL,
		TimerstampCounter ROWVERSION,
		CONSTRAINT PK_QuickOffer PRIMARY KEY (QuickOfferID)
	)
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'QuickOfferID')
BEGIN
	ALTER TABLE Customer ADD QuickOfferID INT NULL
	ALTER TABLE Customer ADD CONSTRAINT FK_Customer_QuickOffer FOREIGN KEY (QuickOfferID) REFERENCES QuickOffer(QuickOfferID)
END
GO

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
@QuickOfferID INT OUTPUT
AS
BEGIN
	INSERT INTO QuickOffer(Amount, CreationDate, ExpirationDate, Aml, BusinessScore, IncorporationDate, TangibleEquity, TotalCurrentAssets)
		VALUES (@Amount, GETUTCDATE(), DATEADD(hour, dbo.udfQuickOfferDuration(), GETUTCDATE()), @Aml, @BusinessScore, @IncorporationDate, @TangibleEquity, @TotalCurrentAssets)

	SET @QuickOfferID = SCOPE_IDENTITY()

	UPDATE Customer SET QuickOfferID = @QuickOfferID WHERE Id = @CustomerID
END
GO
