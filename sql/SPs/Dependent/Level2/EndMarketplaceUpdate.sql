IF OBJECT_ID('EndMarketplaceUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE EndMarketplaceUpdate AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE EndMarketplaceUpdate
@MarketplaceID INT,
@HistoryRecordID INT,
@MarketplaceTypeID UNIQUEIDENTIFIER,
@UpdatingEnd DATETIME,
@ErrorMessage NVARCHAR(MAX),
@TokenExpired INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @Amazon UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @eBay UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @EKM UNIQUEIDENTIFIER = '57ABA690-EDBA-4D95-89CF-13A34B40E2F1'
	DECLARE @FreeAgent UNIQUEIDENTIFIER = '737691E8-5C77-48EF-B01B-7348E24094B6'
	DECLARE @HMRC UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
	DECLARE @PayPal UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'
	DECLARE @Sage UNIQUEIDENTIFIER = '4966BB57-0146-4E3D-AA24-F092D90B7923'
	DECLARE @Yodlee UNIQUEIDENTIFIER = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'

	------------------------------------------------------------------------------

	DECLARE @Bigcommerce UNIQUEIDENTIFIER = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996'
	DECLARE @KashFlow UNIQUEIDENTIFIER = 'A755B4F6-D4EC-4D80-96A2-B2849BD800AC'
	DECLARE @Magento UNIQUEIDENTIFIER = 'A660B9CC-8BB1-4A37-9597-507622AEBF9E'
	DECLARE @Play UNIQUEIDENTIFIER = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6'
	DECLARE @Prestashop UNIQUEIDENTIFIER = 'AE0BC89A-9884-4025-9D96-2755A6CD10EE'
	DECLARE @Shopify UNIQUEIDENTIFIER = 'A386F349-8E41-4BA9-B709-90332466D42D'
	DECLARE @Volusion UNIQUEIDENTIFIER = 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C'
	DECLARE @Xero UNIQUEIDENTIFIER = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C'

	------------------------------------------------------------------------------

	UPDATE MP_CustomerMarketPlace SET
		UpdatingEnd = @UpdatingEnd,
		UpdateError = @ErrorMessage,
		TokenExpired = @TokenExpired
	WHERE
		Id = @MarketplaceID

	------------------------------------------------------------------------------

	SET @ErrorMessage = LTRIM(RTRIM(ISNULL(@ErrorMessage, '')))

	IF @ErrorMessage = ''
		SET @ErrorMessage = NULL

	------------------------------------------------------------------------------

	UPDATE MP_CustomerMarketPlaceUpdatingHistory SET
		UpdatingEnd = @UpdatingEnd,
		Error = @ErrorMessage
	WHERE
		Id = @HistoryRecordID

	------------------------------------------------------------------------------

	IF @ErrorMessage IS NOT NULL
	BEGIN
		IF @MarketplaceTypeID = @Amazon
			EXECUTE UpdateMpTotalsAmazon @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @eBay
			EXECUTE UpdateMpTotalsEbay @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @EKM
			EXECUTE UpdateMpTotalsEkm @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @FreeAgent
			EXECUTE UpdateMpTotalsFreeAgent @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @HMRC
			EXECUTE UpdateMpTotalsHmrc @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @PayPal
			EXECUTE UpdateMpTotalsPayPal @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @Sage
			EXECUTE UpdateMpTotalsSage @HistoryRecordID
		ELSE IF @MarketplaceTypeID = @Yodlee
			EXECUTE UpdateMpTotalsYodlee @HistoryRecordID
		ELSE IF @MarketplaceTypeID IN (@Bigcommerce, @KashFlow, @Magento, @Play, @Prestashop, @Shopify, @Volusion, @Xero)
			EXECUTE UpdateMpTotalsChaGra @HistoryRecordID
	END
END
GO
