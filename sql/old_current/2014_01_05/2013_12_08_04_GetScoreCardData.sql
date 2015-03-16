IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetScoreCardData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetScoreCardData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetScoreCardData] 
	(@CustomerId INT)
AS
BEGIN
	
	SELECT
		c.MaritalStatus AS MaritalStatus, 
		FeedBack.MaxFeedback, 
		--Market places Num
		(
		SELECT 
			count(cmp.Id) 
		FROM 
			MP_CustomerMarketPlace cmp 
			LEFT JOIN MP_MarketplaceType mpt ON mpt.Id = cmp.MarketPlaceId
		WHERE 
			cmp.customerId = c.Id AND 
			mpt.InternalId IN ('A7120CB7-4C93-459B-9901-0E95E7281B59', 'A4920125-411F-4BB9-A52D-27E8A00D0A3B', '3FA5E327-FCFD-483B-BA5A-DC1815747A28')
		) AS MPsNumber,
		-- EZBOB seniority
		ISNULL(DateDiff(DAY, c.GreetingMailSentDate, GETDATE()), 0) AS EZBOBSeniority,
		-- EZBOB #of previous ON time loans
		(
		SELECT 
			count(l.id) 
		FROM 
			Loan l 
			LEFT JOIN LoanSchedule ls ON ls.LoanId = l.Id
		WHERE 
			l.CustomerId = c.Id AND 
			(
				ls.Status = 'PaidOnTime' OR 
				ls.Status = 'PaidEarly'
			) AND 
			ls.Id = 
				(
				SELECT 
					MAX(ls.id) 
				FROM 
					Customer c
					LEFT JOIN LoanSchedule ls ON ls.LoanId = l.Id
					LEFT JOIN Loan l ON c.Id = l.CustomerId
				WHERE 
					l.CustomerId = c.Id
				) 
		) AS OnTimeLoans,
		-- EZBOB #of previous late payments
		(
		SELECT 
			count(lc.id) 
		FROM 
			LoanCharges lc 
			INNER JOIN ConfigurationVariables cv ON cv.Id = lc.ConfigurationVariableId 
			INNER JOIN Loan l ON l.Id = lc.LoanId
		WHERE 
			cv.Name = 'LatePaymentCharge' AND
			l.CustomerId = @CustomerId
		) AS LatePayments,
		-- EZBOB #of previous early payments
		(
		SELECT 
			count(ls.id) 
		FROM 
			LoanSchedule ls 
			INNER JOIN Loan l ON l.Id = ls.LoanId
		WHERE 
			ls.Status = 'PaidEarly' AND
			c.Id = l.CustomerId
		) AS EarlyPayments,
		-- EZBOB #of previous early payments 
		(
		SELECT 
			Min(ls.Date) 
		FROM 
			LoanSchedule ls
			INNER JOIN Loan l ON l.Id = ls.LoanId
		WHERE c.Id = l.CustomerId
		) AS FirstRepaymentDate

	FROM Customer c
	LEFT JOIN

	-- Positive feedback counter (lifetime)
	(
		SELECT 
			FeedbackTable.CustomerId AS CustomerId,
			sum(FeedbackTable.Feedback) AS MaxFeedback
		FROM
		(
			SELECT 
				c.Id AS CustomerId,
				cmp.Id AS umi, 
				fbi.Positive AS Feedback
			-- Positive eBay feedback (lifetime) 
			FROM 
				MP_CustomerMarketPlace cmp
				LEFT JOIN Customer c ON cmp.CustomerId = c.Id
				LEFT JOIN MP_AmazonFeedback fb ON 
					fb.CustomerMarketPlaceId=cmp.Id AND
					fb.Created = 
					(
						SELECT 
							MAX(fb1.Created) 
						FROM 
							MP_AmazonFeedback fb1
						WHERE 
							fb1.CustomerMarketPlaceId = fb.CustomerMarketPlaceId
					)  
				LEFT JOIN MP_AmazonFeedbackItem fbi ON 
					fbi.AmazonFeedbackId = fb.Id AND 
					fbi.TimePeriodId = 5

			UNION 
			
			SELECT  
				c.Id AS CustomerId,
				cmp.Id AS umi,
				fbi2.Positive AS Feedback
			-- Positive Amazon feedback (lifetime)  
			FROM 
				MP_CustomerMarketPlace cmp
				LEFT JOIN Customer c ON cmp.CustomerId = c.Id
				LEFT JOIN MP_EbayFeedback fb2 ON 
					fb2.CustomerMarketPlaceId=cmp.Id AND
					fb2.Created = 
					(
						SELECT 
							MAX(fb1.Created) 
						FROM 
							MP_EbayFeedback fb1
						WHERE 
						fb1.CustomerMarketPlaceId = fb2.CustomerMarketPlaceId
					) 
				LEFT JOIN MP_EbayFeedbackItem fbi2 ON 
					fbi2.EbayFeedbackId = fb2.Id AND
					fbi2.TimePeriodId = 6
		) AS FeedbackTable
		GROUP BY 
			FeedbackTable.CustomerId
	) AS FeedBack ON c.Id = Feedback.CustomerId
	WHERE 
		c.Id = @CustomerId
END
GO
