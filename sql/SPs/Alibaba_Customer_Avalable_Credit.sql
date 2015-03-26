IF OBJECT_ID('AlibabaCustomerAvalableCredit') IS NULL
	EXECUTE('CREATE PROCEDURE AlibabaCustomerAvalableCredit AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[AlibabaCustomerAvalableCredit]
	@CustomerID  int, @AliMemberId bigint
AS
BEGIN
	--
	SET NOCOUNT ON;
	
	IF (SELECT Id from dbo.Customer where Id = @CustomerID)  IS NULL
	BEGIN
		SELECT NULL AS aId, @AliMemberId as aliMemberId ;
	END

	IF (SELECT Id from dbo.AlibabaBuyer where AliId = @AliMemberId) IS NULL
	BEGIN
		SELECT NULL AS aliMemberId, @CustomerID as aId  ;
		RETURN ;
	END


	IF (SELECT CustomerId from dbo.AlibabaBuyer where AliId = @AliMemberId) <> @CustomerID
	BEGIN
		SELECT NULL AS aliMemberId, NULL as aId;  -- mismatch customerID and Ali memberID
		RETURN ;
	END	

	DECLARE  @USD_RATE DECIMAL(18,2);

	-- USD last rate
	SET @USD_RATE = (select top 1 Price as rate from [dbo].[MP_Currency] where  Name = 'USD' order by LastUpdated desc) ;

	-- Available credit = (Approved amount - Freeze - Loan taken + Repayment)
	
	DECLARE @VALID_CREDITLINE_EXISTS bit

	SELECT top 1 			
			@CustomerID as aId, 		
			r.UnderwriterDecision as lineStatus,		 
			r.UnderwriterDecisionDate as lastUpdate,
			r.ManagerApprovedSum as creditLine,
			(r.ManagerApprovedSum - ll.loanOpened) as unusedCreditAmount,		 
			CASE WHEN r.ManagerApprovedSum IS NOT NULL THEN r.ManagerApprovedSum * @USD_RATE ELSE NULL END AS creditLine_USD,
			CASE WHEN (r.ManagerApprovedSum - ll.loanOpened) IS NOT NULL THEN (r.ManagerApprovedSum - ll.loanOpened) * @USD_RATE ELSE NULL END AS unusedCreditAmount_USD,		  		
			r.OfferStart,	
			r.OfferValidUntil,			 	
			a.AliId as aliMemberId,
			@VALID_CREDITLINE_EXISTS as validCashRequest
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


		if @VALID_CREDITLINE_EXISTS IS NULL
		SELECT @AliMemberId AS aliMemberId, @CustomerID as aId;  

END