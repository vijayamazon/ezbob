DECLARE @CustomerID int; SET @CustomerID = 371;

select * from NL_CashRequests cr
join 
(select MAX([DecisionID]) as lastDesicionID, d.CashRequestID as CRID from [dbo].[NL_Decisions] d join [dbo].[NL_CashRequests] cr1 on d.CashRequestID=cr1.CashRequestID and cr1.CustomerID=CustomerID group by d.CashRequestID) dd 
on dd.CRID=cr.CashRequestID
left join [dbo].[NL_Offers] o on o.DecisionID=dd.lastDesicionID
where CustomerID=CustomerID

select * from  NL_CashRequests cr join [dbo].[NL_Decisions] d on d.CashRequestID=cr.CashRequestID and cr.CustomerID=CustomerID;

