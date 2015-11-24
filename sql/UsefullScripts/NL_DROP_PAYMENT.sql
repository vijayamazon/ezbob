/****** Script for SelectTopNRows command from SSMS  ******/

DECLARE @PaymentID bigint = 18;
delete from [dbo].[NL_LoanSchedulePayments] where PaymentID=(select PaymentID from [dbo].[NL_Payments] where [PaymentID] = @PaymentID);
delete from [dbo].[NL_LoanFeePayments] where PaymentID=(select PaymentID from [dbo].[NL_Payments] where [PaymentID] = @PaymentID);
delete from [dbo].[NL_Payments] where [PaymentID] = @PaymentID;

