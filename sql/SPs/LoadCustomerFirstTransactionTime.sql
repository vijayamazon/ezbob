SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCustomerFirstTransactionTime') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerFirstTransactionTime AS SELECT 1')
GO

ALTER PROCEDURE LoadCustomerFirstTransactionTime
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @Amazon       UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @eBay         UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @EKM          UNIQUEIDENTIFIER = '57ABA690-EDBA-4D95-89CF-13A34B40E2F1'
	DECLARE @HMRC         UNIQUEIDENTIFIER = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
	DECLARE @PayPal       UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'

	DECLARE @Bigcommerce  UNIQUEIDENTIFIER = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996'
	DECLARE @Magento      UNIQUEIDENTIFIER = 'A660B9CC-8BB1-4A37-9597-507622AEBF9E'
	DECLARE @Play         UNIQUEIDENTIFIER = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6'
	DECLARE @Prestashop   UNIQUEIDENTIFIER = 'AE0BC89A-9884-4025-9D96-2755A6CD10EE'
	DECLARE @Shopify      UNIQUEIDENTIFIER = 'A386F349-8E41-4BA9-B709-90332466D42D'
	DECLARE @Volusion     UNIQUEIDENTIFIER = 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C'

	------------------------------------------------------------------------------
	--
	-- Find customer's marketplaces.
	--
	------------------------------------------------------------------------------

	SELECT
		MpID = m.Id,
		MpTypeID = t.InternalId,
		FirstTransactionTime = CONVERT(DATETIME, NULL),
		m.OriginationDate
	INTO
		#mp
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id
	WHERE
		m.CustomerId = @CustomerID
		AND
		ISNULL(m.Disabled, 0) = 0
		AND
		t.InternalId IN (
			@Amazon, @eBay, @EKM, @HMRC, @PayPal,
			@Bigcommerce, @Magento, @Play, @Prestashop, @Shopify, @Volusion
		)

	------------------------------------------------------------------------------
	--
	-- A note.
	--
	------------------------------------------------------------------------------

	-- If customer has more than one Amazon marketplace then all the Amazon
	-- entries in #mp table will have the same first transaction time. This
	-- is not an issue because we want first transaction time accross all
	-- the marketplaces no matter what their type is.

	-- This idea holds for all the marketplace types, not only Amazon.

	-- So, when the output query is executed #mp table contains in every row:
	-- 1. marketplace id
	-- 2. marketplace type id
	-- 3. first transaction time for that marketplace TYPE.

	------------------------------------------------------------------------------
	--
	-- Process Amazon.
	--
	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM #mp WHERE MpTypeID = @Amazon)
	BEGIN
		UPDATE #mp SET
			FirstTransactionTime = (
				SELECT
					MIN(i.PurchaseDate)
				FROM
					#mp mp
					INNER JOIN MP_AmazonOrder o ON mp.MpId = o.CustomerMarketPlaceId
					INNER JOIN MP_AmazonOrderItem i ON o.Id = i.AmazonOrderId
				WHERE
					mp.MpTypeID = @Amazon
					AND
					i.PurchaseDate IS NOT NULL
			)
		WHERE
			MpTypeID = @Amazon
	END

	------------------------------------------------------------------------------
	--
	-- Process Channel Grabber.
	--
	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM #mp WHERE MpTypeID IN (@Bigcommerce, @Magento, @Play, @Prestashop, @Shopify, @Volusion))
	BEGIN
		UPDATE #mp SET
			FirstTransactionTime = (
				SELECT
					MIN(i.PaymentDate)
				FROM
					#mp mp
					INNER JOIN MP_ChannelGrabberOrder o ON mp.MpId = o.CustomerMarketPlaceId
					INNER JOIN MP_ChannelGrabberOrderItem i ON o.Id = i.OrderId
				WHERE
					mp.MpTypeID IN (@Bigcommerce, @Magento, @Play, @Prestashop, @Shopify, @Volusion)
					AND
					i.PaymentDate IS NOT NULL
			)
		WHERE
			MpTypeID IN (@Bigcommerce, @Magento, @Play, @Prestashop, @Shopify, @Volusion)
	END

	------------------------------------------------------------------------------
	--
	-- Process HMRC.
	--
	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM #mp WHERE MpTypeID = @HMRC)
	BEGIN
		UPDATE #mp SET
			FirstTransactionTime = (
				SELECT
					MIN(dbo.udfMinDate(o.DateFrom, o.DateTo))
				FROM
					#mp mp
					INNER JOIN MP_VatReturnRecords o ON mp.MpId = o.CustomerMarketPlaceId
				WHERE
					mp.MpTypeID = @HMRC
					AND
					ISNULL(o.IsDeleted, 0) = 0
			)
		WHERE
			MpTypeID = @HMRC
	END

	------------------------------------------------------------------------------
	--
	-- Process eBay
	--
	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM #mp WHERE MpTypeID = @eBay)
	BEGIN
		UPDATE #mp SET
			FirstTransactionTime = (
				SELECT
					MIN(o.RegistrationDate)
				FROM
					#mp mp
					INNER JOIN MP_EbayUserData o ON mp.MpId = o.CustomerMarketPlaceId
				WHERE
					mp.MpTypeID = @eBay
					AND
					o.RegistrationDate IS NOT NULL
			)
		WHERE
			MpTypeID = @eBay
	END

	------------------------------------------------------------------------------
	--
	-- Process EKM
	--
	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM #mp WHERE MpTypeID = @EKM)
	BEGIN
		UPDATE #mp SET
			FirstTransactionTime = (
				SELECT
					MIN(i.OrderDate)
				FROM
					#mp mp
					INNER JOIN MP_EkmOrder o ON mp.MpId = o.CustomerMarketPlaceId
					INNER JOIN MP_EkmOrderItem i ON o.Id = i.OrderId
				WHERE
					mp.MpTypeID = @EKM
					AND
					i.OrderDate IS NOT NULL
			)
		WHERE
			MpTypeID = @EKM
	END

	------------------------------------------------------------------------------
	--
	-- Process Pay Pal
	--
	------------------------------------------------------------------------------

	IF EXISTS (SELECT * FROM #mp WHERE MpTypeID = @PayPal)
	BEGIN
		UPDATE #mp SET
			FirstTransactionTime = (
				SELECT
					MIN(i.Created)
				FROM
					#mp mp
					INNER JOIN MP_PayPalTransaction o ON mp.MpId = o.CustomerMarketPlaceId
					INNER JOIN MP_PayPalTransactionItem2 i ON o.Id = i.TransactionId
				WHERE
					mp.MpTypeID = @PayPal
					AND
					i.Created IS NOT NULL
			)
		WHERE
			MpTypeID = @PayPal
	END

	------------------------------------------------------------------------------
	--
	-- Output.
	--
	------------------------------------------------------------------------------

	SELECT
		FirstTransactionTime = MIN(dbo.udfMinDate(FirstTransactionTime, OriginationDate))
	FROM
		#mp
	WHERE
		FirstTransactionTime IS NOT NULL
		OR
		OriginationDate IS NOT NULL

	------------------------------------------------------------------------------
	--
	-- Cleanup.
	--
	------------------------------------------------------------------------------

	DROP TABLE #mp

	------------------------------------------------------------------------------
END
GO
