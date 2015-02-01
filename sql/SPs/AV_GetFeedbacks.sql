SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetFeedbacks') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetFeedbacks AS SELECT 1')
GO

ALTER PROCEDURE AV_GetFeedbacks
@CustomerId INT
AS
BEGIN

DECLARE @AmazonFeedbacks INT
DECLARE @EbayFeedbacks INT
DECLARE @PaypalFeedbacks INT
DECLARE @DefaultFeedbacks INT

--amazon positive feedbacks
;WITH AF AS (
	SELECT af.CustomerMarketPlaceId, max(af.Id) FeedbackId
	FROM MP_CustomerMarketPlace mp INNER JOIN MP_AmazonFeedback af ON mp.Id = af.CustomerMarketPlaceId
	WHERE mp.CustomerId=@CustomerId AND mp.Disabled=0
	GROUP BY af.CustomerMarketPlaceId
)
SELECT @AmazonFeedbacks = isnull(sum(afi.Positive), 0)
FROM MP_AmazonFeedbackItem afi INNER JOIN AF ON afi.AmazonFeedbackId = AF.FeedbackId
INNER JOIN MP_AnalysisFunctionTimePeriod tp ON tp.Id = afi.TimePeriodId
WHERE tp.InternalId = '3A552C6D-C28D-4D5B-9590-7D4A8094BD0A' -- lifetime

--ebay positive feedbacks
;WITH EF AS (
	SELECT
		ef.CustomerMarketPlaceId,
		MAX(ef.Id) FeedbackId
	FROM
		MP_CustomerMarketPlace mp
		INNER JOIN MP_EbayFeedback ef
			ON mp.Id = ef.CustomerMarketPlaceId
	WHERE
		mp.CustomerId = @CustomerId
		AND
		mp.Disabled = 0
	GROUP BY
		ef.CustomerMarketPlaceId
)
SELECT
	@EbayFeedbacks = ISNULL(SUM(efi.Positive), 0)
FROM
	MP_EbayFeedbackItem efi
	INNER JOIN EF
		ON efi.EbayFeedbackId = EF.FeedbackId
	INNER JOIN
		MP_AnalysisFunctionTimePeriod tp
			ON tp.Id = efi.TimePeriodId
	WHERE
		tp.InternalId = '16619B19-ABF5-4AE0-9040-93EFD2E71FDB' -- 0 all data

--num of paypal income transactions
SELECT
	@PaypalFeedbacks = COUNT(*) 
FROM
	MP_CustomerMarketPlace mp
	INNER JOIN MP_PayPalTransaction ppt
		ON mp.Id = ppt.CustomerMarketPlaceId
	INNER JOIN MP_PayPalTransactionItem2 ppti
		ON ppt.Id = ppti.TransactionId
WHERE
	mp.CustomerId = @CustomerId
	AND
	mp.Disabled = 0
	AND
	ppti.GrossAmount > 0

-- default value
SELECT @DefaultFeedbacks = isnull(CAST(Value AS INT), 0) FROM ConfigurationVariables WHERE Name = 'DefaultFeedbackValue'

SELECT @AmazonFeedbacks AS AmazonFeedbacks, @EbayFeedbacks AS EbayFeedbacks, @PaypalFeedbacks AS PaypalFeedbacks, @DefaultFeedbacks AS DefaultFeedbacks

END 
GO
