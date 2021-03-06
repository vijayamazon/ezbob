IF OBJECT_ID('LoadCustomerMarketplaceOriginationTimes') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerMarketplaceOriginationTimes AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerMarketplaceOriginationTimes
@CustomerID INT,
@Now DATETIME = NULL
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'common'
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
		AND
		(@Now IS NULL OR cmp.Created < @Now)

	UNION
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
		TwoTime          = MIN(r.DateTo),
		Source           = 'hmrc'
	FROM
		MP_VatReturnRecords r
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
		AND (
			ISNULL(r.IsDeleted, 0) = 0
			OR
			(@Now IS NOT NULL AND NOT EXISTS (
				SELECT h.HistoryItemID
				FROM MP_VatReturnRecordDeleteHistory h
				WHERE h.DeletedRecordID = r.Id
				AND h.DeletedTime < @Now
			))
		)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'amazon'
	FROM
		MP_AmazonOrder r
		INNER JOIN MP_AmazonOrderItem i ON r.Id = i.AmazonOrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = MIN(i.PaymentDate),
		Source           = 'chagra'
	FROM
		MP_ChannelGrabberOrder r
		INNER JOIN MP_ChannelGrabberOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'ebay'
	FROM
		MP_EbayUserData r
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR cmp.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
	------------------------------------------------------------------------------
	--
	-- EKM
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.OrderDate),
		TwoTime          = MIN(i.OrderDateIso),
		Source           = 'ekm'
	FROM
		MP_EkmOrder r
		INNER JOIN MP_EkmOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'free agent - invoices'
	FROM
		MP_FreeAgentRequest r
		INNER JOIN MP_FreeAgentInvoice i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
	------------------------------------------------------------------------------
	--
	-- FreeAgent - Expenses
	--
	------------------------------------------------------------------------------

	SELECT
		RowType          = @OriginationTime,
		MarketplaceID    = cmp.Id,
		MarketplaceType  = mt.Name,
		IsPaymentAccount = mt.IsPaymentAccount,
		InternalID       = mt.InternalID,
		OneTime          = MIN(i.dated_on),
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'free agent - expenses'
	FROM
		MP_FreeAgentRequest r
		INNER JOIN MP_FreeAgentExpense i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'pay pal'
	FROM
		MP_PayPalTransaction r
		INNER JOIN MP_PayPalTransactionItem2 i ON r.Id = i.TransactionId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR cmp.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'paypoint'
	FROM
		MP_PayPointOrder r
		INNER JOIN MP_PayPointOrderItem i ON r.Id = i.OrderId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'sage - invoices'
	FROM
		MP_SageRequest r
		INNER JOIN MP_SageSalesInvoice i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'sage - purchase'
	FROM
		MP_SageRequest r
		INNER JOIN MP_SagePurchaseInvoice i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'sage - incomes'
	FROM
		MP_SageRequest r
		INNER JOIN MP_SageIncome i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = CONVERT(DATETIME, NULL),
		Source           = 'sage - exp'
	FROM
		MP_SageRequest r
		INNER JOIN MP_SageExpenditure i ON r.Id = i.RequestId
		INNER JOIN MP_CustomerMarketPlace cmp
			ON r.CustomerMarketPlaceId = cmp.Id
			AND ISNULL(cmp.Disabled, 0) = 0
			AND cmp.CustomerId = @CustomerID
		INNER JOIN MP_MarketplaceType mt
			ON cmp.MarketPlaceId = mt.Id
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	UNION
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
		TwoTime          = MIN(ISNULL(t.transactionDate, t.postDate)),
		Source           = 'yodlee'
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
	WHERE
		(@Now IS NULL OR r.Created < @Now)
	GROUP BY
		cmp.Id,
		mt.Name,
		mt.IsPaymentAccount,
		mt.InternalID

	------------------------------------------------------------------------------
END
GO
