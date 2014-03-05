IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_ReRejectionData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_ReRejectionData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_ReRejectionData] 
	(@CustomerId INT,
@CashRequestId INT)
AS
BEGIN
	--decision date
  DECLARE @AutomaticDecisionDate DATETIME = (SELECT SystemDecisionDate FROM CashRequests WHERE Id=@CashRequestId)
  
  --manual reject date
  DECLARE @ManualRejectDate DATETIME = (SELECT max(UnderwriterDecisionDate)
  										FROM CashRequests 
  										WHERE IdCustomer=@CustomerId 
  										AND IdUnderwriter IS NOT NULL 
  										AND UnderwriterDecision='Rejected')
  
  --is new client
  DECLARE @IsNewClient NVARCHAR(5) = 'True'
  IF EXISTS (SELECT * FROM Loan WHERE CustomerId=@CustomerId) SET @IsNewClient = 'False'
  
  --new data source was added
  DECLARE @NewDataSourceAdded NVARCHAR(5) = 'False'
  IF EXISTS (SELECT * FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId AND Created > @ManualRejectDate) SET @NewDataSourceAdded = 'True'
  
  --old client principal repayment 
  DECLARE @RepaidAmount DECIMAL = 0
  DECLARE @LoanAmount INT = 0
  IF @IsNewClient = 'False'
  BEGIN
  	 SET @RepaidAmount = (SELECT sum(lt.LoanRepayment) 
	 FROM Loan l LEFT JOIN LoanTransaction lt ON lt.LoanId = l.Id
	 WHERE l.Customerid = @CustomerId 
	 AND lt.Status = 'Done' 
	 AND	Type ='PaypointTransaction')

	 SET @LoanAmount = (SELECT sum(l.LoanAmount) FROM Loan l WHERE l.Customerid = @CustomerId)	
  END  
  
  SELECT @AutomaticDecisionDate AS AutomaticDecisionDate, @ManualRejectDate AS ManualRejectDate, @IsNewClient AS IsNewClient, @NewDataSourceAdded AS NewDataSourceAdded, @RepaidAmount AS RepaidAmount, @LoanAmount AS LoanAmount
END
GO
