IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAdsReport]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAdsReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAdsReport] 
	(@time DATETIME)
AS
BEGIN
	SELECT
		ReferenceSource,
		count(1) TotalUsers,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(CreditSum))), 1), 2) TotalCredit
	FROM
		Customer
	WHERE
		CONVERT(DATE, @time) <= GreetingMailSentDate
	GROUP BY
		ReferenceSource
	ORDER BY
		ReferenceSource
END
GO
