IF OBJECT_ID('GetCustomerAvalableCredit') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerAvalableCredit AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[GetCustomerAvalableCredit]
	@CustomerEmail  NVARCHAR(255)
AS
BEGIN
	
	SET NOCOUNT ON;

	-- Available credit = (Approved amount - Freeze - Loan taken + Repayment)
	DECLARE @CustomerID INT;

	SELECT @CustomerID = Id from dbo.Customer where Name = @CustomerEmail;	

	IF @CustomerID IS NULL
	BEGIN
		RETURN
	END

	SELECT top 1 			
		 @CustomerID as CustomerID, 		
		 r.UnderwriterDecision as Decision,		 
		 CONVERT(VARCHAR(10),r.UnderwriterDecisionDate,110) as DecisionDate,		
		 (r.ManagerApprovedSum - ll.loanOpened) as  Amount, 
		 r.UnderwriterComment as Comment ,	
		 r.OfferStart,
		 r.OfferValidUntil,		 
		 r.ApprovedRepaymentPeriod,		 
		 r.InterestRate,			
		 r.AutoDecisionID,		
		 r.Originator,
		 a.AliId,
		 a.Freeze	
	  FROM 
		dbo.CashRequests r left join dbo.AlibabaBuyer a on r.IdCustomer = a.CustomerId
		left join 
			(select 
				ISNULL((SUM(l.LoanAmount) - SUM(t.LoanRepayment)), 0) as loanOpened,	
				l.CustomerId 			
			from 
				dbo.Loan as l left join dbo.LoanTransaction as t on l.Id = t.LoanId 
			where 
				l.Status <> 'PaidOff' and t.Type like 'PaypointTransaction' and t.Status = 'Done' and l.CustomerId = @CustomerID
			group by l.CustomerId
			) as ll on ll.CustomerId = r.IdCustomer		
		
		where r.UnderwriterDecision is not null and r.IdCustomer= @CustomerID and r.OfferValidUntil >= GETDATE() 
		
		 	  	 
	  order by r.Id desc 
  
END
