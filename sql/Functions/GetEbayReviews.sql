IF OBJECT_ID (N'dbo.GetEbayReviews') IS NOT NULL
	DROP FUNCTION dbo.GetEbayReviews
GO

CREATE FUNCTION [dbo].[GetEbayReviews]()
RETURNS TABLE 
AS
RETURN 
(
	select cmp.CustomerId,
	 (afi.Negative + afi.Positive + afi.Neutral) as reviews,
	 rating=
	 case when (afi.Negative + afi.Positive + afi.Neutral) = 0
	 then 0
	 else (afi.Positive*100)/(afi.Negative + afi.Positive + afi.Neutral)
	 end
	 from (select m.CustomerMarketPlaceId, af.Id from  dbo.MP_EbayFeedback as af INNER JOIN (select MAX(Created) Created, CustomerMarketPlaceId CustomerMarketPlaceId from dbo.MP_EbayFeedback GROUP BY CustomerMarketPlaceId) as m
ON af.Created = m.Created and af.CustomerMarketPlaceId = m.CustomerMarketPlaceId) as af
Left outer join dbo.MP_EbayFeedbackItem as afi on
af.Id = afi.EbayFeedbackId and afi.TimePeriodId = 4
Left outer join dbo.MP_CustomerMarketPlace as cmp on 
cmp.Id = af.CustomerMarketPlaceId
)

GO

