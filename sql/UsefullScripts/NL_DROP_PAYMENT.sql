
DECLARE @PaymentID bigint = 35;
delete from [dbo].[NL_PaypointTransactions] where PaymentID=@PaymentID;
delete from [dbo].[NL_LoanSchedulePayments] where PaymentID=@PaymentID;
delete from [dbo].[NL_LoanFeePayments] where PaymentID=@PaymentID;
delete from [dbo].[NL_Payments] where [PaymentID] = @PaymentID;

