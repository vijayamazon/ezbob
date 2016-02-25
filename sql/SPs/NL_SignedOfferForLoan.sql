SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_SignedOfferForLoan') IS NULL
	EXECUTE('CREATE PROCEDURE NL_SignedOfferForLoan AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_SignedOfferForLoan] 
	@CustomerID INT, 
	@Now DATETIME
AS
BEGIN 

	-- prevent loan creation in less than 1 minute.
	DECLARE @LastLoanDate DATETIME;

	SET @LastLoanDate = (SELECT max( lh.EventTime )
	FROM NL_Loans l
	JOIN vw_NL_LoansCustomer v on v.LoanID = l.LoanID
	JOIN NL_LoanHistory lh ON lh.LoanID = l.LoanID
	where v.CustomerID = @CustomerID);
	
	IF @LastLoanDate IS NOT NULL AND DATEDIFF(SECOND, @LastLoanDate, @now) < 60		
	BEGIN			
		select 0 as OfferID, 'Loan delay violation' as Error ;
		RETURN 
	END

	IF object_id('#validOffer') IS NOT NULL DROP TABLE #validOffer;

	DECLARE @NumofAllowedActiveLoans INT;
	SET @NumofAllowedActiveLoans = (SELECT Value from ConfigurationVariables where Name = 'NumofAllowedActiveLoans')

	DECLARE @LoansCount int;

	-- NUMBER OF ACTIVE LOANS
	SET @LoansCount = (SELECT COUNT(l.LoanID) 
		from vw_NL_LoansCustomer v
		JOIN NL_Loans l on l.LoanID = v.LoanID
		JOIN NL_LoanStatuses ls ON ls.LoanStatusID = l.LoanStatusID 
		where v.CustomerID =  @CustomerID
		AND ls.LoanStatus <> 'PaidOff')

	IF @LoansCount IS NOT NULL AND @LoansCount >= @NumofAllowedActiveLoans 
	BEGIN 
		select 0 as OfferID, 'Max loans limit' as Error; --from @validOffer 
		RETURN 
	END
		
	select top 1 o.OfferID,
			o.LoanTypeID,
			o.RepaymentIntervalTypeID,
			o.LoanSourceID,
			o.RepaymentCount AS OfferRepaymentCount,
			o.Amount AS OfferAmount,
			o.MonthlyInterestRate,
			o.SetupFeeAddedToLoan,
			BrokerSetupFeePercent = CASE WHEN ll.LoanLegalID IS NOT NULL THEN ll.BrokerSetupFeePercent ELSE o.BrokerSetupFeePercent END,
			o.InterestOnlyRepaymentCount,
			o.DiscountPlanID,
			ll.LoanLegalID,
			ll.Amount AS LoanLegalAmount,
			ll.RepaymentPeriod AS LoanLegalRepaymentPeriod,
			@LoansCount AS LoansCount,
			0 AS AvailableAmount,
			ExistsRefnums = (Select distinct(Refnum)+ ',' AS [text()] From NL_Loans For XML PATH ('')),
			o.IsRepaymentPeriodSelectionAllowed	
	INTO #validOffer	
	from (select * from 
	((select MAX(d1.[DecisionID]) as lastDesicionID, d1.CashRequestID as CRID from [dbo].[NL_Decisions] d1 
		join [dbo].[NL_CashRequests] cr1 on d1.CashRequestID=cr1.CashRequestID and cr1.CustomerID=@CustomerID 
		join Decisions dn on d1.DecisionNameID = dn.DecisionID and dn.DecisionName IN ('Approve','ReApprove') group by d1.CashRequestID) dd1
		join (select MAX(OfferID) as lastOffer, DecisionID from [dbo].[NL_Offers] where @Now BETWEEN StartTime AND EndTime group by DecisionID) of1 on of1.DecisionID=dd1.lastDesicionID)) maxOffer
		join NL_Offers o on o.OfferID=maxOffer.lastOffer
		join NL_LoanLegals ll on ll.OfferID=maxOffer.lastOffer and ll.LoanLegalID = (select MAX(LoanLegalID) from NL_LoanLegals where OfferID=maxOffer.lastOffer)
	order by o.OfferID desc;


	--select * from #validOffer;
	--return

	IF ((SELECT IsRepaymentPeriodSelectionAllowed from #validOffer) = 0
	   AND (SELECT OfferRepaymentCount from #validOffer) <> (SELECT LoanLegalRepaymentPeriod from #validOffer))	 
	BEGIN 
		 select 0 as OfferID, 'Wrong repayment period' as Error;
		 RETURN
	END

	-- loan period is in the range of default period allowed by loan source
	IF (select OfferRepaymentCount from  #validOffer) < (select DefaultRepaymentPeriod from  #validOffer JOIN LoanSource ls ON #validOffer.LoanSourceID = ls.LoanSourceID) 
	BEGIN 
		 select 0 as OfferID, 'Wrong repayment period' as Error;
		 RETURN
	END

	-- loan interest is in the range of max interest allowed by loan source
	IF (select MonthlyInterestRate from  #validOffer) >	(select MaxInterest from  #validOffer JOIN LoanSource ls ON #validOffer.LoanSourceID = ls.LoanSourceID) 
	BEGIN 
		 select 0 as OfferID, 'Wrong interest rate' as Error;
		 RETURN
	END
	
	-- available credit for current offer
	declare @TakenAmount decimal;
	set @TakenAmount = (select sum(x.loansAmount) FROM 
						(select min(lh.Amount) as loansAmount
							FROM NL_LoanHistory lh
							JOIN NL_Loans l on l.LoanID = lh.LoanID 
							JOIN NL_LoanStatuses lstatus on	lstatus.LoanStatusID = l.LoanStatusID AND lstatus.LoanStatus in ('Pending', 'Live', 'Late')
							where l.OfferID = (SELECT OfferID from #validOffer)
				group by lh.LoanID) x)

	IF @TakenAmount IS NOT NULL	
	BEGIN
		update #validOffer set AvailableAmount = (select OfferAmount from #validOffer) - @TakenAmount		
	END	ELSE
	BEGIN
		update #validOffer set AvailableAmount  = (select OfferAmount from #validOffer)		
	END

	SELECT * FROM #validOffer

END