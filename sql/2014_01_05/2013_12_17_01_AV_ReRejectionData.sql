IF OBJECT_ID('AV_ReRejectionData') IS NULL
	EXECUTE('CREATE PROCEDURE AV_ReRejectionData AS SELECT 1')
GO

ALTER PROCEDURE AV_ReRejectionData
@CustomerId INT
AS
BEGIN 
  --DECLARE @CustomerId INT = 14223
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
  
  SELECT @ManualRejectDate AS ManualRejectDate, @IsNewClient AS IsNewClient, @NewDataSourceAdded AS NewDataSourceAdded, @RepaidAmount AS RepaidAmount, @LoanAmount AS LoanAmount
END 
GO 

