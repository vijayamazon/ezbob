IF OBJECT_ID('GetCustomerTurnoverData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerTurnoverData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerTurnoverData
@CustomerID INT,
@MonthCount INT,
@DateTo DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @MpID INT
	DECLARE @MpType UNIQUEIDENTIFIER

	------------------------------------------------------------------------------

	DECLARE @eBay        UNIQUEIDENTIFIER = 'A7120CB7-4C93-459B-9901-0E95E7281B59'
	DECLARE @Amazon      UNIQUEIDENTIFIER = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B'
	DECLARE @EKM         UNIQUEIDENTIFIER = '57ABA690-EDBA-4D95-89CF-13A34B40E2F1'
	
	DECLARE @Volusion    UNIQUEIDENTIFIER = 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C'
	DECLARE @Play        UNIQUEIDENTIFIER = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6'
	DECLARE @Shopify     UNIQUEIDENTIFIER = 'A386F349-8E41-4BA9-B709-90332466D42D'
	DECLARE @Magento     UNIQUEIDENTIFIER = 'A660B9CC-8BB1-4A37-9597-507622AEBF9E'
	DECLARE @Prestashop  UNIQUEIDENTIFIER = 'AE0BC89A-9884-4025-9D96-2755A6CD10EE'
	DECLARE @Bigcommerce UNIQUEIDENTIFIER = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996'

	DECLARE @PayPal      UNIQUEIDENTIFIER = '3FA5E327-FCFD-483B-BA5A-DC1815747A28'

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
		AND (
			mt.IsPaymentAccount = 0
			OR
			mt.InternalId = @PayPal
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
		ELSE IF @MpType IN (@Volusion, @Play, @Shopify, @Magento, @Prestashop, @Bigcommerce)
			EXECUTE GetMpTurnoverChaGra @MpID, @MonthCount, @DateTo

		FETCH NEXT FROM curMp INTO @MpID, @MpType
	END

	------------------------------------------------------------------------------

	CLOSE curMp

	------------------------------------------------------------------------------

	DEALLOCATE curMp
END
GO
