IF OBJECT_ID('GetScoreCardData') IS NULL
	EXECUTE ('CREATE PROCEDURE GetScoreCardData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetScoreCardData
@CustomerId INT,
@Today DATE
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Feedback INT

	------------------------------------------------------------------------------

	CREATE TABLE #Feedback (Feedback INT NULL)

	------------------------------------------------------------------------------
	
	SELECT
		cmp.Id AS CustomerMarketPlaceId,
		MAX(fb.Created) AS MaxCreated
	INTO
		#MaxAmazonCreated
	FROM
		MP_AmazonFeedback fb
		INNER JOIN MP_CustomerMarketPlace cmp ON fb.CustomerMarketPlaceId = cmp.Id
	WHERE
		cmp.CustomerId = @CustomerId
	GROUP BY
		cmp.Id

	------------------------------------------------------------------------------

	INSERT INTO #Feedback (Feedback)
	SELECT
		fbi.Positive -- Positive eBay feedback (lifetime)
	FROM
		MP_AmazonFeedback fb
		INNER JOIN #MaxAmazonCreated mc
			ON fb.CustomerMarketPlaceId = mc.CustomerMarketPlaceId
			AND fb.Created = mc.MaxCreated
		INNER JOIN MP_AmazonFeedbackItem fbi
			ON fbi.AmazonFeedbackId = fb.Id
			AND fbi.TimePeriodId = 5

	------------------------------------------------------------------------------

	SELECT
		cmp.Id AS CustomerMarketPlaceId,
		MAX(fb.Created) AS MaxCreated
	INTO
		#MaxEbayCreated
	FROM
		MP_EbayFeedback fb
		INNER JOIN MP_CustomerMarketPlace cmp ON fb.CustomerMarketPlaceId = cmp.Id
	WHERE
		cmp.CustomerId = @CustomerId
	GROUP BY
		cmp.Id

	------------------------------------------------------------------------------

	INSERT INTO #Feedback (Feedback)
	SELECT
		fbi.Positive -- Positive eBay feedback (lifetime)
	FROM
		MP_EbayFeedback fb
		INNER JOIN #MaxEbayCreated mc
			ON fb.CustomerMarketPlaceId = mc.CustomerMarketPlaceId
			AND fb.Created = mc.MaxCreated
		INNER JOIN MP_EbayFeedbackItem fbi
			ON fbi.EbayFeedbackId = fb.Id
			AND fbi.TimePeriodId = 6
	
	------------------------------------------------------------------------------

	SELECT @Feedback = SUM(Feedback) FROM #Feedback

	------------------------------------------------------------------------------

	SELECT
		c.MaritalStatus AS MaritalStatus, 
		@Feedback AS MaxFeedback, 
		--Market places Num
		(
			SELECT COUNT(cmp.Id)
			FROM MP_CustomerMarketPlace cmp 
			LEFT JOIN MP_MarketplaceType mpt ON mpt.Id = cmp.MarketPlaceId
			WHERE cmp.customerId = @CustomerId
			AND mpt.InternalId IN (
				'A7120CB7-4C93-459B-9901-0E95E7281B59',
				'A4920125-411F-4BB9-A52D-27E8A00D0A3B',
				'3FA5E327-FCFD-483B-BA5A-DC1815747A28'
			)
		) AS MPsNumber,
		-- EZBOB seniority
		ISNULL(DateDiff(DAY, c.GreetingMailSentDate, @Today), 0) AS EZBOBSeniority,
		-- EZBOB #of previous ON time loans
		(
			SELECT COUNT(l.id)
			FROM Loan l
			INNER JOIN LoanSchedule ls ON ls.LoanId = l.Id
			WHERE l.CustomerId = @CustomerId
			AND ls.Status IN ('PaidOnTime', 'PaidEarly')
			AND ls.Id = (
				SELECT MAX(ls.id)
				FROM LoanSchedule ls
				INNER JOIN Loan l ON ls.LoanId = l.Id
				WHERE l.CustomerId = @CustomerId
			)
		) AS OnTimeLoans,
		-- EZBOB #of previous late payments
		(
			SELECT COUNT(lc.id)
			FROM LoanCharges lc 
			INNER JOIN ConfigurationVariables cv ON cv.Id = lc.ConfigurationVariableId
			INNER JOIN Loan l ON l.Id = lc.LoanId
			WHERE cv.Name = 'LatePaymentCharge'
			AND l.CustomerId = @CustomerId
		) AS LatePayments,
		-- EZBOB #of previous early payments
		(
			SELECT COUNT(ls.id) 
			FROM LoanSchedule ls 
			INNER JOIN Loan l ON l.Id = ls.LoanId
			WHERE ls.Status = 'PaidEarly'
			AND l.CustomerId = @CustomerId
		) AS EarlyPayments,
		-- EZBOB #of previous early payments 
		(
			SELECT MIN(ls.Date) 
			FROM LoanSchedule ls
			INNER JOIN Loan l ON l.Id = ls.LoanId
			WHERE l.CustomerId = @CustomerId
		) AS FirstRepaymentDate
	FROM
		Customer c
	WHERE 
		c.Id = @CustomerId

	DROP TABLE #MaxEbayCreated
	DROP TABLE #MaxAmazonCreated
	DROP TABLE #Feedback
END
GO
