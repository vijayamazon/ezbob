IF OBJECT_ID('GetCustomerTurnoverData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerTurnoverData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerTurnoverData
@OnlineOnly BIT,
@CustomerID INT,
@MonthCount INT,
@DateTo DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MpID INT
	DECLARE @MpType UNIQUEIDENTIFIER

	------------------------------------------------------------------------------

	DECLARE @eBay         UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @Amazon       UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @EKM          UNIQUEIDENTIFIER = '57ABA690-EDBA-4D95-89CF-13A34B40E2F1'
	
	DECLARE @Volusion     UNIQUEIDENTIFIER = 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C'
	DECLARE @Play         UNIQUEIDENTIFIER = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6'
	DECLARE @Shopify      UNIQUEIDENTIFIER = 'A386F349-8E41-4BA9-B709-90332466D42D'
	DECLARE @Magento      UNIQUEIDENTIFIER = 'A660B9CC-8BB1-4A37-9597-507622AEBF9E'
	DECLARE @Prestashop   UNIQUEIDENTIFIER = 'AE0BC89A-9884-4025-9D96-2755A6CD10EE'
	DECLARE @Bigcommerce  UNIQUEIDENTIFIER = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996'

	DECLARE @PayPal       UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'

	DECLARE @Xero         UNIQUEIDENTIFIER = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C'
	DECLARE @KashFlow     UNIQUEIDENTIFIER = 'A755B4F6-D4EC-4D80-96A2-B2849BD800AC'

	DECLARE @Yodlee       UNIQUEIDENTIFIER = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
	DECLARE @FreeAgent    UNIQUEIDENTIFIER = '737691E8-5C77-48EF-B01B-7348E24094B6'
	DECLARE @Sage         UNIQUEIDENTIFIER = '4966BB57-0146-4E3D-AA24-F092D90B7923'
	DECLARE @HMRC         UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'

	DECLARE @PayPoint     UNIQUEIDENTIFIER = 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0'
	DECLARE @CompanyFiles UNIQUEIDENTIFIER = '1C077670-6D6C-4CE9-BEBC-C1F9A9723908'

	------------------------------------------------------------------------------

	DECLARE curMp CURSOR FOR
	SELECT
		cmp.Id,
		mt.InternalId
	FROM
		MP_CustomerMarketPlace cmp
		INNER JOIN MP_MarketplaceType mt ON cmp.MarketPlaceId = mt.Id
	WHERE
		cmp.CustomerId = @CustomerID
		AND
		ISNULL(cmp.Disabled, 0) = 0
		AND
		mt.InternalId NOT IN (@PayPoint, @CompanyFiles)
		AND (
			(@OnlineOnly = 1 AND mt.InternalId IN (@eBay, @Amazon, @PayPal, @HMRC))
			OR
			@OnlineOnly = 0
		)

	------------------------------------------------------------------------------

	OPEN curMp

	------------------------------------------------------------------------------

	FETCH NEXT FROM curMp INTO @MpID, @MpType

	------------------------------------------------------------------------------

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @MpType = @eBay
			EXECUTE GetMpTurnoverEbay @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @Amazon
			EXECUTE GetMpTurnoverAmazon @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @EKM
			EXECUTE GetMpTurnoverEkm @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @PayPal
			EXECUTE GetMpTurnoverPayPal @MpID, @MonthCount, @DateTo
		ELSE IF @MpType IN (@Volusion, @Play, @Shopify, @Magento, @Prestashop, @Bigcommerce, @Xero, @Kashflow)
			EXECUTE GetMpTurnoverChaGra @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @Yodlee
			EXECUTE GetMpTurnoverYodlee @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @FreeAgent
			EXECUTE GetMpTurnoverFreeAgent @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @Sage
			EXECUTE GetMpTurnoverSage @MpID, @MonthCount, @DateTo
		ELSE IF @MpType = @HMRC
			EXECUTE GetMpTurnoverHmrc @MpID, @MonthCount, @DateTo

		FETCH NEXT FROM curMp INTO @MpID, @MpType
	END

	------------------------------------------------------------------------------

	CLOSE curMp

	------------------------------------------------------------------------------

	DEALLOCATE curMp
END
GO
