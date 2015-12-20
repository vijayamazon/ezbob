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

	-- valid offer	
	SELECT top 1 
		ll.LoanLegalID,
		ll.Amount AS LoanLegalAmount,
		ll.RepaymentPeriod AS LoanLegalRepaymentPeriod,
		o.OfferID,
		o.LoanTypeID,
		o.RepaymentIntervalTypeID,
		o.LoanSourceID,
		o.RepaymentCount AS OfferRepaymentCount,
		o.Amount AS OfferAmount,
		o.MonthlyInterestRate,
		o.SetupFeeAddedToLoan,
		o.BrokerSetupFeePercent,
		o.InterestOnlyRepaymentCount,
		o.DiscountPlanID,
		@LoansCount AS LoansCount,
		0 AS AvailableAmount,
		ExistsRefnums = (Select distinct(Refnum)+ ',' AS [text()] From NL_Loans For XML PATH ('')),
		o.IsRepaymentPeriodSelectionAllowed
	INTO #validOffer				   		
	FROM NL_Offers o
	JOIN NL_Decisions d ON d.DecisionID = o.DecisionID
	JOIN NL_CashRequests cr ON cr.CashRequestID = d.CashRequestID
	JOIN Decisions dn ON d.DecisionNameID = dn.DecisionID	
	JOIN NL_LoanLegals ll ON o.OfferID = ll.OfferID
	WHERE cr.CustomerID = 371
		AND '2015 20 December 19:10' BETWEEN o.StartTime AND o.EndTime
		AND dn.DecisionName IN ('Approve','ReApprove') 
		and ll.LoanLegalID = (select MAX( ll1.LoanLegalID) from NL_LoanLegals ll1 where ll1.OfferID=o.OfferID )
	ORDER BY o.OfferID DESC ;

	IF ((SELECT IsRepaymentPeriodSelectionAllowed from #validOffer) = 0
	   AND (SELECT OfferRepaymentCount from #validOffer) <> (SELECT LoanLegalRepaymentPeriod from #validOffer))	 
	BEGIN 
		 select 0 as OfferID, 'Wrong repayment period' as Error;
		 RETURN
	END

	-- loan period is in the range of max period allowed by loan source
	IF (select OfferRepaymentCount from  #validOffer) >
	(select DefaultRepaymentPeriod from  #validOffer JOIN LoanSource ls ON #validOffer.LoanSourceID = ls.LoanSourceID) 
	BEGIN 
		 select 0 as OfferID, 'Wrong repayment period' as Error;
		 RETURN
	END

	-- loan interest is in the range of max interest allowed by loan source
	IF (select MonthlyInterestRate from  #validOffer) >
	(select MaxInterest from  #validOffer JOIN LoanSource ls ON #validOffer.LoanSourceID = ls.LoanSourceID) 
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