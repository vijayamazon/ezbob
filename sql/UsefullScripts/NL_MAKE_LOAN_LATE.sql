declare @monthBack int; set @monthBack = -8;
declare @loanID int; set @loanID = 5103;
declare @NLloanID bigint; set @NLloanID = (select LoanID from NL_Loans where OldLoanID = @loanID);

if @NLloanID is null return;
if @NLloanID =0 return;

declare @offerID bigint; set @offerID = (select OfferID from NL_Loans where [LoanID] = @NLloanID);
declare @historyID bigint; set @historyID = (select LoanHistoryID from NL_LoanHistory where LoanID = @NLloanID);

--select @NLloanID
update [dbo].[NL_FundTransfers] set [TransferTime] = DATEADD(DAY, @monthBack, [TransferTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanFees] set [CreatedTime] = DATEADD(DAY, @monthBack,[CreatedTime]),[AssignTime]= DATEADD(DAY, @monthBack,[AssignTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanHistory] set [EventTime]=DATEADD(DAY, @monthBack,[EventTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanInterestFreeze] set 
	[StartDate]=DATEADD(DAY, @monthBack,[StartDate]),
	[EndDate]=DATEADD(DAY, @monthBack,[EndDate]),
	[ActivationDate]= DATEADD(DAY, @monthBack,[ActivationDate]), 
	[DeactivationDate] = DATEADD(DAY, @monthBack, [DeactivationDate]) where [LoanID] = @NLloanID;
update [dbo].[NL_LoanLegals] set [SignatureTime] = DATEADD(DAY, @monthBack,[SignatureTime]) where [OfferID] = @offerID;

update [dbo].[NL_LoanOptions] set [InsertDate]= DATEADD(DAY, @monthBack,[InsertDate]),
	 [StopAutoChargeDate] =DATEADD(DAY, @monthBack,[StopAutoChargeDate]),
	 [StopLateFeeFromDate]=DATEADD(DAY, @monthBack,[StopLateFeeFromDate]),
	 [StopLateFeeToDate]=DATEADD(DAY, @monthBack,[StopLateFeeToDate])  where [LoanID] = @NLloanID;

update [dbo].[NL_LoanRollovers] set 
	[CreationTime]=DATEADD(DAY, @monthBack, [CreationTime]),
	[ExpirationTime]= DATEADD(DAY, @monthBack, [ExpirationTime]),
	[CustomerActionTime]=DATEADD(DAY, @monthBack, [CustomerActionTime]),
	[DeletionTime]=DATEADD(DAY, @monthBack, [DeletionTime]) where [LoanHistoryID] = @historyID;

update [dbo].[NL_Loans] set [CreationTime] = DATEADD(DAY, @monthBack, [CreationTime]) where [LoanID] = @NLloanID;

update [dbo].[NL_LoanSchedules] set [PlannedDate]= DATEADD(DAY, @monthBack, [PlannedDate]), [ClosedTime]=DATEADD(DAY, @monthBack, [ClosedTime]) where [LoanHistoryID] = @historyID;

update [dbo].[NL_Offers] set 
	[StartTime] =DATEADD(DAY, @monthBack, [StartTime]), 
	[EndTime]=DATEADD(DAY, @monthBack, [EndTime]),
	[CreatedTime]=DATEADD(DAY, @monthBack, [CreatedTime]) where OfferID=@offerID;
update [dbo].[NL_PacnetTransactions] set 
	[TransactionTime]=DATEADD(DAY, @monthBack, 
	[TransactionTime]),[StatusUpdatedTime]=DATEADD(DAY, @monthBack, [StatusUpdatedTime]) where [FundTransferID]= (select FundTransferID from NL_FundTransfers where LoanID=@loanID);
	 
update [dbo].[NL_Payments] set [PaymentTime]=DATEADD(DAY, @monthBack, [PaymentTime]), 
	[CreationTime]=DATEADD(DAY, @monthBack, [CreationTime]),
	[DeletionTime]=DATEADD(DAY, @monthBack, [DeletionTime]) where LoanID=@loanID;

update [dbo].[NL_PaypointTransactions] set [TransactionTime]=DATEADD(DAY, @monthBack, [TransactionTime]) where [PaymentID]=(select PaymentID from NL_Payments where LoanID=@loanID);

--return;

--- EXISTING LOAN------------

--TODO add rollovers table

select * from [dbo].[Loan] where Id = @loanID; 

update Loan set [Date] = DATEADD(DAY, @monthBack, [Date]), [Modified] = 1 where Id = @loanID;

update [dbo].[LoanBrokerCommission] set [CreateDate] = DATEADD(DAY, @monthBack, [CreateDate])  where [LoanID] = @loanID;

update [dbo].[LoanChangesHistory] set [Date] = DATEADD(DAY, @monthBack, [Date]) where [LoanId] = @loanID;

update [dbo].[LoanCharges] set [Date] = DATEADD(DAY, @monthBack, [Date]) where [LoanId] = @loanID;

update [dbo].[LoanHistory] set [Date] = DATEADD(DAY, @monthBack, [Date]) where [LoanId] = @loanID;

update [dbo].[LoanInterestFreeze] set 
	[StartDate] = DATEADD(DAY, @monthBack, [StartDate]), 
	[EndDate] = DATEADD(DAY, @monthBack,[EndDate]),
	[ActivationDate] = DATEADD(DAY, @monthBack, [ActivationDate]), 
	[DeactivationDate] = DATEADD(DAY, @monthBack,[DeactivationDate]) where [LoanId] = @loanID;

update [dbo].[LoanLegal] set [Created] = DATEADD(DAY, @monthBack, [Created])  where [LoanId] = @loanID;

update [dbo].[LoanSchedule] set 
	[Date] = DATEADD(DAY, @monthBack, [Date]),
	[DatePaid] = DATEADD(DAY, @monthBack, [Date]),
	[CustomInstallmentDate]=DATEADD(DAY, @monthBack,[CustomInstallmentDate]) where [LoanId] = @loanID;

update [dbo].[LoanScheduleTransaction] set [Date] = DATEADD(DAY, @monthBack, [Date]) where [LoanID] =@loanID;

update [dbo].[LoanTransaction] set [PostDate] = DATEADD(DAY, @monthBack, [PostDate]) where [LoanId] =@loanID;

update [dbo].[LoanOptions] set 
	[StopAutoChargeDate]= DATEADD(DAY, @monthBack, [StopAutoChargeDate]),
	[StopLateFeeFromDate]= DATEADD(DAY, @monthBack, [StopLateFeeFromDate]),
	[StopLateFeeToDate]= DATEADD(DAY, @monthBack, [StopLateFeeToDate]) where [LoanId] =@loanID;





