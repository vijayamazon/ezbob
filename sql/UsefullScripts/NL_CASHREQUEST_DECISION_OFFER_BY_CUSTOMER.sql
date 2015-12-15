DECLARE @CustomerID int; SET @CustomerID = 371;

--select * from [dbo].[NL_Offers] o join [dbo].[NL_Decisions] d on o.DecisionID = d.DecisionID join [dbo].[NL_CashRequests] cr on cr.CashRequestID = d.CashRequestID
--where cr.CustomerID = @CustomerID order by cr.[CashRequestID] desc ;

SELECT 	 
	   d.DecisionID, 
	  --d.DecisionNameID, 
	  dn.DecisionName , d.DecisionTime, d.Notes, d.Position, d.UserID	
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
  FROM [dbo].[NL_Offers] o join [dbo].[NL_Decisions] d on
   o.DecisionID = (select MAX(d1.DecisionID) from  [dbo].[NL_Decisions] d1 join [dbo].[NL_CashRequests] cr on cr.[CashRequestID] = d1.[CashRequestID] where cr.CustomerID = @CustomerID)
   join [dbo].[Decisions] dn on dn.DecisionID = d.DecisionNameID 
  order by d.DecisionTime desc 