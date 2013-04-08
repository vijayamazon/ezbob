IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAdsReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAdsReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAdsReport]
	@time DATETIME
AS
BEGIN
    SELECT 
    	ReferenceSource,
    	count(1) TotalUsers,
    	parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(CreditSum)))),1),2) TotalCredit 
    FROM 
    	Customer 
    WHERE 
    	GreetingMailSentDate >= @time 
    GROUP BY 
    	ReferenceSource 
    ORDER BY 
    	ReferenceSource
END
GO
