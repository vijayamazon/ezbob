DECLARE @CustomerID int;
SET @CustomerID = 351;

select * from [dbo].[NL_Offers] o join [dbo].[NL_Decisions] d on o.DecisionID = d.DecisionID join [dbo].[NL_CashRequests] cr on cr.CashRequestID = d.CashRequestID
where cr.CustomerID = @CustomerID order by cr.[CashRequestID] desc ;