declare @monthBack int; set @monthBack = -1;
declare @loanID int; set @loanID = 5130;
declare @NLloanID bigint; set @NLloanID = (select LoanID from NL_Loans where OldLoanID = @loanID);

if @NLloanID is null return;
if @NLloanID =0 return;

declare @offerID bigint; set @offerID = (select OfferID from NL_Loans where [LoanID] = @NLloanID);
declare @historyID bigint; set @historyID = (select LoanHistoryID from NL_LoanHistory where LoanID = @NLloanID);

--select @NLloanID
update [dbo].[NL_FundTransfers] set [TransferTime] = DATEADD(MONTH, @monthBack, [TransferTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanFees] set [CreatedTime] = DATEADD(MONTH, @monthBack,[CreatedTime]),[AssignTime]= DATEADD(MONTH, @monthBack,[AssignTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanHistory] set [EventTime]=DATEADD(MONTH, @monthBack,[EventTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_Payments] set 
	[PaymentTime]=DATEADD(MONTH, @monthBack, [PaymentTime]), 
	[CreationTime]=DATEADD(MONTH, @monthBack, [CreationTime]),
	[DeletionTime]=DATEADD(MONTH, @monthBack, [DeletionTime]) where LoanID=@NLloanID;

update [dbo].[NL_LoanInterestFreeze] set 
	[StartDate]=DATEADD(MONTH, @monthBack,[StartDate]),
	[EndDate]=DATEADD(MONTH, @monthBack,[EndDate]),
	[ActivationDate]= DATEADD(MONTH, @monthBack,[ActivationDate]), 
	[DeactivationDate] = DATEADD(MONTH, @monthBack, [DeactivationDate]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanLegals] set [SignatureTime] = DATEADD(MONTH, @monthBack,[SignatureTime]) where [OfferID] = @offerID;

update [dbo].[NL_LoanOptions] set [InsertDate]= DATEADD(MONTH, @monthBack,[InsertDate]),
	 [StopAutoChargeDate] =DATEADD(MONTH, @monthBack,[StopAutoChargeDate]),
	 [StopLateFeeFromDate]=DATEADD(MONTH, @monthBack,[StopLateFeeFromDate]),
	 [StopLateFeeToDate]=DATEADD(MONTH, @monthBack,[StopLateFeeToDate])  where [LoanID] = @NLloanID;

update [dbo].[NL_LoanRollovers] set 
	[CreationTime]=DATEADD(MONTH, @monthBack, [CreationTime]),
	[ExpirationTime]= DATEADD(MONTH, @monthBack, [ExpirationTime]),
	[CustomerActionTime]=DATEADD(MONTH, @monthBack, [CustomerActionTime]),
	[DeletionTime]=DATEADD(MONTH, @monthBack, [DeletionTime]) where [LoanHistoryID] = @historyID;

update [dbo].[NL_Loans] set [CreationTime] = DATEADD(MONTH, @monthBack, [CreationTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanSchedules] set [PlannedDate]= DATEADD(MONTH, @monthBack, [PlannedDate]), [ClosedTime]=DATEADD(MONTH, @monthBack, [ClosedTime]) where [LoanHistoryID] = @historyID;

update [dbo].[NL_Offers] set 
	[StartTime] =DATEADD(MONTH, @monthBack, [StartTime]), 
	[EndTime]=DATEADD(MONTH, @monthBack, [EndTime]),
	[CreatedTime]=DATEADD(MONTH, @monthBack, [CreatedTime]) where OfferID=@offerID;

update [dbo].[NL_PacnetTransactions] set 
	[TransactionTime]=DATEADD(MONTH, @monthBack, [TransactionTime]),
	[StatusUpdatedTime]=DATEADD(MONTH, @monthBack, [StatusUpdatedTime]) where [FundTransferID]= (select FundTransferID from NL_FundTransfers where LoanID=@loanID); 


update [dbo].[NL_PaypointTransactions] set [TransactionTime]=DATEADD(MONTH, @monthBack, [TransactionTime]) where [PaymentID]=(select PaymentID from NL_Payments where LoanID=@loanID);

--return;

--- EXISTING LOAN------------

--TODO add rollovers table

select * from [dbo].[Loan] where Id = @loanID; 

update Loan set [Date] = DATEADD(MONTH, @monthBack, [Date]), [Modified] = 1 where Id = @loanID;

update [dbo].[LoanBrokerCommission] set [CreateDate] = DATEADD(MONTH, @monthBack, [CreateDate])  where [LoanID] = @loanID;

update [dbo].[LoanChangesHistory] set [Date] = DATEADD(MONTH, @monthBack, [Date]) where [LoanId] = @loanID;

update [dbo].[LoanCharges] set [Date] = DATEADD(MONTH, @monthBack, [Date]) where [LoanId] = @loanID;

update [dbo].[LoanHistory] set [Date] = DATEADD(MONTH, @monthBack, [Date]) where [LoanId] = @loanID;

update [dbo].[LoanInterestFreeze] set 
	[StartDate] = DATEADD(MONTH, @monthBack, [StartDate]), 
	[EndDate] = DATEADD(MONTH, @monthBack,[EndDate]),
	[ActivationDate] = DATEADD(MONTH, @monthBack, [ActivationDate]), 
	[DeactivationDate] = DATEADD(MONTH, @monthBack,[DeactivationDate]) where [LoanId] = @loanID;

update [dbo].[LoanLegal] set [Created] = DATEADD(MONTH, @monthBack, [Created])  where [LoanId] = @loanID;

update [dbo].[LoanSchedule] set 
	[Date] = DATEADD(MONTH, @monthBack, [Date]),
	[DatePaid] = DATEADD(MONTH, @monthBack, [Date]),
	[CustomInstallmentDate]=DATEADD(MONTH, @monthBack,[CustomInstallmentDate]) where [LoanId] = @loanID;

update [dbo].[LoanScheduleTransaction] set [Date] = DATEADD(MONTH, @monthBack, [Date]) where [LoanID] =@loanID;

update [dbo].[LoanTransaction] set [PostDate] = DATEADD(MONTH, @monthBack, [PostDate]) where [LoanId] =@loanID;

update [dbo].[LoanOptions] set 
	[StopAutoChargeDate]= DATEADD(MONTH, @monthBack, [StopAutoChargeDate]),
	[StopLateFeeFromDate]= DATEADD(MONTH, @monthBack, [StopLateFeeFromDate]),
	[StopLateFeeToDate]= DATEADD(MONTH, @monthBack, [StopLateFeeToDate]) where [LoanId] =@loanID;

update [dbo].[CashRequests] set
	[CreationDate]= DATEADD(MONTH, @monthBack, [CreationDate]),
	[OfferStart] = DATEADD(MONTH, @monthBack, [OfferStart]),
	[OfferValidUntil] = DATEADD(MONTH, @monthBack, [OfferValidUntil])
where IdCustomer = (select [CustomerId] from [dbo].[Loan] where Id=@loanID) 
and [dbo].[CashRequests].[Id] = (select MAX([Id]) from [dbo].[CashRequests] where IdCustomer = (select [CustomerId] from [dbo].[Loan] where Id=@loanID));
 

UPDATE PaymentRollover 
SET 
	Created=isnull(DATEADD(MONTH, @monthBack,  Created), null),
	CustomerConfirmationDate = isnull(DATEADD(MONTH, @monthBack, CustomerConfirmationDate), null), 
	ExpiryDate= isnull(DATEADD(MONTH, @monthBack, ExpiryDate), null)
FROM (
    SELECT s.Id FROM PaymentRollover r join LoanSchedule s on s.Id=r.LoanScheduleId where s.LoanId=@loanID) sch
WHERE 
    sch.Id = PaymentRollover.[LoanScheduleId]




