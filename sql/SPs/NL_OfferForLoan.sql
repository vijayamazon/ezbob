SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OfferForLoan') IS NOT NULL
	DROP PROCEDURE NL_OfferForLoan
GO


CREATE PROCEDURE [dbo].[NL_OfferForLoan]	
	@CustomerID INT 
AS

declare @Now datetime;
set @Now = GETUTCDATE();

declare @OfferID int;
declare @DiscountPlanID int;

BEGIN
	
	-- get last valid offer for the customer
	set @OfferID = (select top 1 o.OfferID FROM NL_Offers o INNER JOIN NL_Decisions d ON d.DecisionID = o.DecisionID INNER JOIN NL_CashRequests cr ON cr.CashRequestID = d.CashRequestID
		WHERE cr.CustomerID = @CustomerID and GETUTCDATE() between o.[StartTime] and o.[EndTime] order by OfferID desc);

--	print "OfferID: "+ cast( @OfferID as varchar(25));

--select @OfferID;

	IF @OfferID IS NULL begin
		
		RETURN NULL;
		end;

	IF object_id('#offerforloan') IS NOT NULL drop table #offerforloan;

	IF @OfferID IS NOT NULL BEGIN
		select top 1 		 
			ll.LoanLegalID,
			ll.Amount as LoanLegalAmount, 			
			ll.RepaymentPeriod as LoanLegalRepaymentPeriod,
			dp.DiscountPlanID,		 
			cast(dp.DiscountPlan as nvarchar(max)) as DiscountPlan,
			o.OfferID , 
			o.LoanTypeID, 
			o.RepaymentIntervalTypeID, 
			o.LoanSourceID, 			 
			o.RepaymentCount as OfferRepaymentCount, 
			o.Amount as OfferAmount, 
			o.MonthlyInterestRate,
			o.SetupFeePercent, 
			o.BrokerSetupFeePercent, 
			o.InterestOnlyRepaymentCount			
		into #offerforloan
			FROM NL_LoanLegals ll INNER JOIN NL_Offers o on ll.OfferID = o.OfferID	
				LEFT JOIN NL_DiscountPlans dp on dp.DiscountPlanID = o.DiscountPlanID -- and dp.IsActive = 1 				
			WHERE o.OfferID = @OfferID
			order by ll.LoanLegalID desc; 

		--select * from #offerforloan;

		set @DiscountPlanID = (select DiscountPlanID from #offerforloan) ;

		if @DiscountPlanID is not null begin			
			update #offerforloan set
			 DiscountPlan=(select ','+cast(dpe.InterestDiscount as varchar(11)) AS [text()] From NL_DiscountPlanEntries dpe Where dpe.DiscountPlanID = @DiscountPlanID ORDER BY dpe.PaymentOrder For XML PATH ('')) 
		end;

		select * from #offerforloan;
	END;

	

END