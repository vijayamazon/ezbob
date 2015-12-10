DECLARE @CustomerID int;
SET @CustomerID = 351;

select * from [dbo].[NL_Offers] o join [dbo].[NL_Decisions] d on o.DecisionID = d.DecisionID join [dbo].[NL_CashRequests] cr on cr.CashRequestID = d.CashRequestID
where cr.CustomerID = @CustomerID order by cr.[CashRequestID] desc ;


DECLARE @CustomerID int; SET @CustomerID = 365;
SELECT 	 
	  cr.[CashRequestID], cr.[CustomerID], cr.[RequestTime], cr.[CashRequestOriginID], cro.CashRequestOrigin, cr.[UserID], cr.[OldCashRequestID]
	  ,d.DecisionID, d.DecisionNameID, dn.DecisionName , d.DecisionTime, d.Notes, d.Position, d.UserID	
	 , o.[OfferID]
    -- , o.[DecisionID]     
      ,[RepaymentIntervalTypeID]
      --,[LoanSourceID]
      ,[StartTime]
      ,[EndTime]
      ,[RepaymentCount]
      ,[Amount]
      ,[MonthlyInterestRate]
      ,[CreatedTime]
     -- ,[BrokerSetupFeePercent]
     -- ,[SetupFeeAddedToLoan]
      , o.Notes as OfferNotes
      --,[InterestOnlyRepaymentCount]
      ,[DiscountPlanID]   
  FROM [dbo].[NL_Offers] 
  o join [dbo].[NL_Decisions] d on d.DecisionID =o.DecisionID join [dbo].[Decisions] dn on dn.DecisionID = d.DecisionNameID
  join [dbo].[NL_CashRequests] cr on cr.[CashRequestID] = d.[CashRequestID] join [dbo].[NL_CashRequestOrigins] cro on cro.CashRequestOriginID = cr.CashRequestOriginID
  where cr.CustomerID = @CustomerID
  order by cr.CashRequestID desc 