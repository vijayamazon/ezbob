IF OBJECT_ID('LoadCustomerMarketplaceOriginationTimes') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerMarketplaceOriginationTimes AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerMarketplaceOriginationTimes
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @OriginationTime NVARCHAR(16) = 'OriginationTime'

	------------------------------------------------------------------------------
	--
	-- Common
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = cmp.OriginationDate,
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_CustomerMarketPlace cmp
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		ISNULL(cmp.Disabled, 0) = 0
		AND 
		cmp.CustomerId = @CustomerID
		AND
		cmp.OriginationDate IS NOT NULL

	------------------------------------------------------------------------------
	--
	-- HMRC
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(r.DateFrom),
		TwoTime          = MIN(r.DateTo)
	FROM
		MP_VatReturnRecords r
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		ISNULL(r.IsDeleted, 0) = 0
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Amazon
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.PurchaseDate),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_AmazonOrder r
		INNER JOIN MP_AmazonOrderItem i ON r.Id = i.AmazonOrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Channel Grabber
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.PurchaseDate),
		TwoTime          = MIN(i.PaymentDate)
	FROM
		MP_ChannelGrabberOrder r
		INNER JOIN MP_ChannelGrabberOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- eBay
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(r.RegistrationDate),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_EbayUserData r
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- EKM
	--
	------------------------------------------------------------------------------
--
	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.OrderDate),
		TwoTime          = MIN(i.OrderDateIso)
	FROM
		MP_EkmOrder r
		INNER JOIN MP_EkmOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- FreeAgent - Invoices
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.dated_on),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_FreeAgentRequest r
		INNER JOIN MP_FreeAgentInvoice i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- FreeAgent - Expences
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.dated_on),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_FreeAgentRequest r
		INNER JOIN MP_FreeAgentExpense i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Pay Pal
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.Created),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_PayPalTransaction r
		INNER JOIN MP_PayPalTransactionItem2 i ON r.Id = i.TransactionId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Paypoint
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.[date]),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_PayPointOrder r
		INNER JOIN MP_PayPointOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Sage - Invoices
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.[date]),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_SageRequest r
		INNER JOIN MP_SageSalesInvoice i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Sage - Purchase invoices
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.[date]),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_SageRequest r
		INNER JOIN MP_SagePurchaseInvoice i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Sage - Incomes
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.[date]),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_SageRequest r
		INNER JOIN MP_SageIncome i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Sage - Expenditure
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.[date]),
		TwoTime          = CONVERT(DATETIME, NULL)
	FROM
		MP_SageRequest r
		INNER JOIN MP_SageExpenditure i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
	--
	-- Yodlee
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		-- if at least one of dates exists - good enough
		OneTime          = MIN(ISNULL(t.postDate, t.transactionDate)),
		TwoTime          = MIN(ISNULL(t.transactionDate, t.postDate))
	FROM
		MP_YodleeOrder r
		INNER JOIN MP_YodleeOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_YodleeOrderItemBankTransaction t ON i.Id = t.OrderItemId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
END
GO
