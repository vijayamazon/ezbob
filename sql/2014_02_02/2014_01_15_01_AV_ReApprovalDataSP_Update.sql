ALTER PROCEDURE AV_ReApprovalData
@CustomerId INT,
@CashRequestId INT
AS
BEGIN 
--DECLARE @CustomerId INT = 14223

DECLARE @ManualApproveDate DATETIME = (SELECT max(UnderwriterDecisionDate)
  										FROM CashRequests c
  										WHERE IdCustomer=@CustomerId 
  										AND IdUnderwriter IS NOT NULL 
  										AND UnderwriterDecision='Approved' AND Id <> @CashRequestId)
  										
DECLARE @ManualApproveRequestId int = (SELECT max(Id)
  										FROM CashRequests 
  										WHERE IdCustomer=@CustomerId 
  										AND IdUnderwriter IS NOT NULL 
  										AND UnderwriterDecision='Approved' AND Id <> @CashRequestId)

--offered amount 
DECLARE @OfferedAmount INT = (SELECT ManagerApprovedSum FROM CashRequests WHERE Id=@ManualApproveRequestId)

--is new client (not took a loan before this cash request)
DECLARE @IsNewClient NVARCHAR(5) = 'True'
IF EXISTS(SELECT * FROM Loan WHERE CustomerId=@CustomerId AND RequestCashId<>@ManualApproveRequestId AND RequestCashId<>@CashRequestId) SET @IsNewClient = 'False'

--took a loan from last offer
DECLARE @TookAmountLastRequest INT = (SELECT sum(LoanAmount)  FROM Loan WHERE CustomerId=@CustomerId AND RequestCashId=@ManualApproveRequestId)
DECLARE @TookLoanLastRequest NVARCHAR(5) = 'False'
IF EXISTS (SELECT * FROM Loan WHERE CustomerId=@CustomerId AND RequestCashId=@ManualApproveRequestId) SET @TookLoanLastRequest = 'True'


DECLARE @PrincipalRepaymentsSinceOffer DECIMAL = 0
DECLARE @WasLate NVARCHAR(5) = 'False'
IF @IsNewClient = 'False'
  BEGIN
	 --repayments of principal since the offer
  	 SET @PrincipalRepaymentsSinceOffer = (SELECT sum(lt.LoanRepayment) 
	 FROM Loan l LEFT JOIN LoanTransaction lt ON lt.LoanId = l.Id
	 WHERE l.Customerid = @CustomerId 
	 AND lt.Status = 'Done' 
	 AND Type ='PaypointTransaction'
	 AND lt.PostDate > @ManualApproveDate)
	 
	 --was late
	 IF EXISTS (SELECT * 
	            FROM Loan l LEFT JOIN LoanSchedule ls ON l.Id=ls.LoanId 
	            WHERE l.CustomerId=@CustomerId 
	            AND ls.[Date]>@ManualApproveDate 
	            AND (ls.Status='Late' OR ls.Status='Paid')) SET @WasLate = 'True'
  END  

--new data source was added
DECLARE @NewDataSourceAdded NVARCHAR(5) = 'False'
IF EXISTS (SELECT * FROM MP_CustomerMarketPlace WHERE CustomerId=@CustomerId AND Created > @ManualApproveDate) SET @NewDataSourceAdded = 'True'
  
----------------------------data---------------------------------------
SELECT @ManualApproveDate AS ManualApproveDate, 
       @OfferedAmount AS OfferedAmount, 
       @IsNewClient AS IsNewClient, 
       @TookAmountLastRequest AS TookAmountLastRequest, 
       @TookLoanLastRequest AS TookLoanLastRequest,
       @PrincipalRepaymentsSinceOffer AS PrincipalRepaymentsSinceOffer,
       @WasLate AS WasLate,
       @NewDataSourceAdded AS NewDataSourceAdded
       
END 

GO

