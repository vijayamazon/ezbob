declare @loanID int; set @loanID = 18;
declare @historyID int; set @historyID = (select LoanHistoryID from NL_LoanHistory where LoanID = @loanID);

delete from NL_LoanSchedules where LoanHistoryID =@historyID;
delete from dbo.NL_LoanAgreements where LoanHistoryID=@historyID;
delete from NL_LoanHistory where LoanHistoryID =@historyID;

delete from [dbo].[NL_LoanFeePayments] where PaymentID=(select PaymentID from [dbo].[NL_Payments] where [LoanID] = @loanID);
delete from [dbo].[NL_Payments] where [LoanID] = @loanID;

delete dbo.NL_FundTransfers where LoanID=@loanID;
DELETE FROM NL_PacnetTransactions where FundTransferID = (select FundTransferID from NL_FundTransfers WHERE  LoanID=@loanID);
delete from NL_LoanFees where LoanID=@loanID;
delete from [dbo].[NL_LoanOptions] where LoanID=@loanID;
delete from NL_Loans where LoanID=@loanID;
