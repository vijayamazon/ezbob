SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OfferForLoan') IS NULL
	EXECUTE('CREATE PROCEDURE NL_OfferForLoan AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_OfferForLoan]	
	@CustomerID INT,
	@Now DATETIME
AS

declare @OfferID int;

BEGIN
	
	-- get last valid 'Approve' or 'ReApprove' offer for the customer
	set @OfferID = (select top 1 o.OfferID FROM NL_Offers o 
	INNER JOIN NL_Decisions d ON d.DecisionID = o.DecisionID 
	INNER JOIN NL_CashRequests cr ON cr.CashRequestID = d.CashRequestID
	inner join [dbo].[Decisions] dnames on d.DecisionNameID = dnames.[DecisionID]
	WHERE cr.CustomerID = @CustomerID and @Now between o.[StartTime] and o.[EndTime] and dnames.[DecisionName] in ('Approve', 'ReApprove') 
	order by OfferID desc);

	IF @OfferID IS NULL begin		
		RETURN NULL;
	end;

	--select @OfferID

	declare @DiscountPlanID int;
	declare @LoansCount int;

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
			o.DistributedSetupFeePercent,
			o.InterestOnlyRepaymentCount,
			0 as LoansCount		
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

		set @LoansCount = (select COUNT(LoanID) 
							from NL_Loans l inner join NL_Offers o on o.OfferID = l.OfferID inner join NL_Decisions d on d.DecisionID=o.DecisionID 
							inner join NL_CashRequests cr on cr.CashRequestID = d.CashRequestID 
							where cr.CustomerID=@CustomerID);

		if @LoansCount is not null begin
			update #offerforloan set LoansCount = @LoansCount;
		end;

		select * from #offerforloan;
	END;
	

END