IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetAmazonFeedback]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetAmazonFeedback]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MP_GetAmazonFeedback] 
	(@iCustomerMarketPlaceId int)
AS
BEGIN
	select Negative, Positive, Neutral 

from MP_CustomerMarketPlace cmp
LEFT JOIN MP_EbayFeedback fb ON fb.CustomerMarketPlaceId=@iCustomerMarketPlaceId
          and fb.Created = (select MAX(fb1.Created)
          from MP_EbayFeedback fb1
          where fb1.CustomerMarketPlaceId = fb.CustomerMarketPlaceId)  
  LEFT JOIN MP_EbayFeedbackItem fbi on fbi.EbayFeedbackId = fb.Id and fbi.TimePeriodId = 4


WHERE cmp.MarketPlaceId=2
END
GO
