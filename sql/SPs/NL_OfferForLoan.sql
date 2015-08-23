SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OfferForLoan') IS NULL
	EXECUTE('CREATE PROCEDURE NL_OfferForLoan AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_OfferForLoan]	
	@CustomerID INT,
	@Now DATETIME
AS

declare @OfferID bigint;

BEGIN
	
	-- get last valid 'Approve' or 'ReApprove' offer for the customer
	set @OfferID = (select top 1 o.OfferID FROM NL_Offers o 
	INNER JOIN NL_Decisions d ON d.DecisionID = o.DecisionID 
	INNER JOIN NL_CashRequests cr ON cr.CashRequestID = d.CashRequestID
	inner join [dbo].[Decisions] dnames on d.DecisionNameID = dnames.[DecisionID]
	WHERE cr.CustomerID = @CustomerID and @Now between o.[StartTime] and o.[EndTime] and dnames.[DecisionName] in ('Approve', 'ReApprove') 
	order by OfferID desc);

	IF @OfferID IS NULL begin		
		RETURN ;
	end;

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
			o.SetupFeeAddedToLoan,			
			o.BrokerSetupFeePercent, 
			o.InterestOnlyRepaymentCount,			
			0 as LoansCount	,
			o.Amount as AvailableAmount
		into #offerforloan
			FROM 
				NL_LoanLegals ll 
				INNER JOIN NL_Offers o on ll.OfferID = o.OfferID					
				LEFT JOIN NL_DiscountPlans dp on dp.DiscountPlanID = o.DiscountPlanID -- and dp.IsActive = 1 				
			WHERE o.OfferID = @OfferID
			order by ll.LoanLegalID desc; 

		set @DiscountPlanID = (select DiscountPlanID from #offerforloan) ;

		--if @DiscountPlanID is not null begin			
		--	update #offerforloan set
		--	 DiscountPlan=(select ','+cast(dpe.InterestDiscount as varchar(11)) AS [text()] From NL_DiscountPlanEntries dpe Where dpe.DiscountPlanID = @DiscountPlanID ORDER BY dpe.PaymentOrder For XML PATH ('')) 
		--end;

		set @LoansCount = (select COUNT(LoanID) 
							from NL_Loans l inner join NL_Offers o on o.OfferID = l.OfferID inner join NL_Decisions d on d.DecisionID=o.DecisionID 
							inner join NL_CashRequests cr on cr.CashRequestID = d.CashRequestID 
							where cr.CustomerID=@CustomerID);	



		if @LoansCount > 0 begin
						
			declare @TakenAmount decimal;
			declare @PaidPrincipal decimal;
			
			-- can be queried [dbo].[NL_LoanStates] ??? TODO check
			set @TakenAmount = 
					(select 
						SUM( h.[Amount]) 
					from 
						NL_Loans l inner join [dbo].[NL_LoanStatuses] lstatus on lstatus.[LoanStatusID] = l.[LoanStatusID] and lstatus.[LoanStatus] in ('Pending', 'Live', 'Late')	
						inner join 	[dbo].[NL_LoanHistory] h on h.LoanID = l.LoanID
						where l.OfferID =  @OfferID);


			if @TakenAmount is null set @TakenAmount =0;

			-- consider use Customer.CreditSum

			-- check already taken and returned loans for this offer
			set @PaidPrincipal = 
					(select 
						SUM(sp.PrincipalPaid)	
					from 
						NL_Loans l inner join [dbo].[NL_LoanHistory] h on l.[LoanID] = h.LoanID 
						inner join [dbo].[NL_LoanSchedules] s on s.[LoanHistoryID] = h.[LoanHistoryID] and s.[ClosedTime] is not null 
						inner join [dbo].[NL_LoanScheduleStatuses] sstatus on sstatus.[LoanScheduleStatusID] = s.[LoanScheduleStatusID] and sstatus.[LoanScheduleStatus] in ('Paid', 'PaidEarly', 'PaidOnTime')		
						inner join [dbo].[NL_LoanSchedulePayments]	sp on sp.LoanScheduleID = s.LoanScheduleID
						inner join [dbo].[NL_Payments] p on p.PaymentID = sp.LoanSchedulePaymentID and p.[DeletionTime] is null
						inner join [dbo].[NL_PaymentStatuses] pstatus on pstatus.[PaymentStatusID] = p.PaymentStatusID and pstatus.PaymentStatus in ('Pending', 'Active')			
					where l.OfferID = @OfferID);

			if @PaidPrincipal is null set @PaidPrincipal =0;

			update #offerforloan set LoansCount = @LoansCount, AvailableAmount = (@TakenAmount- @PaidPrincipal);
		end; -- @LoansCount

		select * from #offerforloan;
	END;

END