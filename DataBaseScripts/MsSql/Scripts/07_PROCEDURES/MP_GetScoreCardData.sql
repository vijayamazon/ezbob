IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_GetScoreCardData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[MP_GetScoreCardData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [MP_GetScoreCardData]
	@CustomerId int, 
	@AnnualSalesTotal int output,
	@MaxFeedback int output,
	@MPsNumber int output,
	@EZBOBSeniority int output,
	@OnTimeLoans int output,
	@LatePayments int output,
	@EarlyPayments int output,
	@MaxMPSeniority int output
	
AS
BEGIN

select  c.Id,
IsNull(AnnualSalesEbay.Total, 0) + IsNull(AnnualSalesAmazon.Total, 0) as AnnualSalesTotal,
 FeedBack.MaxFeedback,
( select count(cmp.Id) from MP_CustomerMarketPlace cmp WHERE cmp.customerId = c.Id ) as MPsNumber,
ISNULL(DateDiff( DAY, c.GreetingMailSentDate, GETDATE()), 0) as EZBOBSeniority,
( select count(l.id) from Loan l where l.CustomerId = c.Id and l.PaymentStatus = 'OnTime') as OnTimeLoans,
 
 ( select count(ls.id) from LoanSchedule ls 
  LEFT JOIN Loan l on l.Id = ls.Id  
  where ls.Status = 'Late'
  and c.Id = l.CustomerId) as LatePayments,
   
   ( select count(ls.id) from LoanSchedule ls 
  LEFT JOIN Loan l on l.Id = ls.Id  
  where ls.Status = 'Early'
  and c.Id = l.CustomerId) as EarlyPayments,
 
 ISNULL(DateDiff( DAY, MarketplaceSeniorityTable.MarketplaceSeniority, GETDATE()), 0) as AccountAge

from Customer c
left join 
(
select TotalTable12.CustomerId as CustomerId,
 SUM(TotalTable12.Price * TotalTable12.Quantity / TotalTable12.Rate) as Total
 from 
  (
    select 
           c.Id as CustomerId,
     et8.price as Price,
           et8.QuantityPurchased as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(eoi8.AmountPaidCurrency, eoi8.CreatedTime) TableRate) as Rate
   from MP_EbayOrderItem eoi8
   left join MP_EbayOrder eo8 on eoi8.OrderId = eo8.Id
   left join MP_EbayTransaction et8 on et8.OrderItemId = eoi8.Id
   left join MP_CustomerMarketPlace cmp ON eo8.CustomerMarketPlaceId = cmp.Id
   left join Customer c on c.Id = cmp.CustomerId
   where (eoi8.OrderStatus LIKE 'Completed' 
    OR eoi8.OrderStatus LIKE 'Authenticated'
    OR eoi8.OrderStatus LIKE 'Shipped')    
 and cmp.EliminationPassed = 1 
    AND DATEADD(MONTH, 12, eoi8.CreatedTime ) >= GETDATE()    
  ) as TotalTable12
 group by CustomerId) as AnnualSalesEbay on c.id = AnnualSalesEbay.CustomerId

left join 
(
select TotalTable3.CustomerId,
SUM(TotalTable3.Price * TotalTable3.Quantity / TotalTable3.Rate) as Total
 from 
   (
    select  c.Id as CustomerId, 
   aoi8.OrderTotal as Price,
           aoi8.NumberOfItemsShipped as Quantity, 
          (select TableRate.Rate from dbo.MP_GetCurrencyRate1(aoi8.OrderTotalCurrency, aoi8.PurchaseDate) TableRate) as Rate
     from  MP_AmazonOrder ao8
    left join MP_AmazonOrderItem2 aoi8 on aoi8.AmazonOrderId = ao8.Id 
    left join MP_CustomerMarketPlace cmp ON  ao8.CustomerMarketPlaceId = cmp.Id
    left join Customer c on c.Id = cmp.CustomerId
    where aoi8.OrderStatus LIKE 'Shipped'   
   AND cmp.EliminationPassed = 1
            AND DATEADD(MONTH, 12, aoi8.PurchaseDate ) >= GETDATE()
  )as TotalTable3 
  group by customerId) as AnnualSalesAmazon on c.Id = AnnualSalesAmazon.CustomerId

left join
(
select FeedbackTable.CustomerId as CustomerId,
Max(FeedbackTable.Feedback) as MaxFeedback
from
(
Select  c.Id as CustomerId,
 cmp.Id as umi,
 
    fbi.Positive as Feedback
 
 FROM MP_CustomerMarketPlace cmp
 LEFT JOIN Customer c  ON cmp.CustomerId = c.Id
 LEFT JOIN MP_AmazonFeedback fb ON fb.CustomerMarketPlaceId=cmp.Id
        and fb.Created = (select MAX(fb1.Created)
        from MP_AmazonFeedback fb1
        where fb1.CustomerMarketPlaceId = fb.CustomerMarketPlaceId)  
 LEFT JOIN MP_AmazonFeedbackItem fbi on fbi.AmazonFeedbackId = fb.Id and fbi.TimePeriodId = 5

 union 

Select  c.Id as CustomerId,
cmp.Id as umi,
 
    fbi2.Positive as Feedback
 
 FROM MP_CustomerMarketPlace cmp
 LEFT JOIN Customer c  ON cmp.CustomerId = c.Id
  LEFT JOIN MP_EbayFeedback fb2 ON fb2.CustomerMarketPlaceId=cmp.Id
          and fb2.Created = (select MAX(fb1.Created)
          from MP_EbayFeedback fb1
          where fb1.CustomerMarketPlaceId = fb2.CustomerMarketPlaceId)  
  LEFT JOIN MP_EbayFeedbackItem fbi2 on fbi2.EbayFeedbackId = fb2.Id and fbi2.TimePeriodId = 6
) as FeedbackTable
group by FeedbackTable.CustomerId
) as FeedBack on c.Id = Feedback.CustomerId
left join 
(
select MarketplacesDatesTable.CustomerId,
MIN(MarketplacesDatesTable.MarketplaceSeniority) as MarketplaceSeniority
from 
(
select cmp.CustomerId,
      aoi.PurchaseDate as MarketplaceSeniority
from  MP_AmazonOrderItem2 aoi
    left join MP_AmazonOrder ao on aoi.AmazonOrderId = ao.Id 
    left join MP_CustomerMarketPlace cmp ON ao.CustomerMarketPlaceId = cmp.CustomerId

union 

select cmp.CustomerId,
  eoi.ShippedTime as MarketplaceSeniority
from MP_EbayOrderItem eoi
left join MP_EbayOrder eo on eoi.OrderId = eo.Id
left join MP_CustomerMarketplace cmp on eo.CustomerMarketPlaceId = cmp.CustomerId
) as MarketplacesDatesTable
group by MarketplacesDatesTable.CustomerId
)as MarketplaceSeniorityTable on MarketplaceSeniorityTable.CustomerId = c.Id


where c.Id = @CustomerId



END
GO
