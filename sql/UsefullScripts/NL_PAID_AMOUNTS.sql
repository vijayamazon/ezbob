DECLARE @LoanID bigint;
set @LoanID = 17;

select * from [dbo].[NL_Payments] where LoanID = @LoanID;

select * from [dbo].[NL_LoanFeePayments] where [PaymentID] in (select PaymentID from [dbo].[NL_Payments] where [LoanID] = @LoanID);
select * from [dbo].[NL_LoanSchedulePayments] where [PaymentID] in (select PaymentID from [dbo].[NL_Payments] where [LoanID] = @LoanID);

-- schedules
select s.*, ss.LoanScheduleStatus from [dbo].[NL_LoanSchedules] s inner join [dbo].[NL_LoanScheduleStatuses] ss on ss.LoanScheduleStatusID= s.LoanScheduleStatusID 
inner join [dbo].[NL_LoanHistory] h on h.LoanHistoryID = s.LoanHistoryID and h.LoanID = @LoanID;

-- fees
select f.*, ft.[LoanFeeType] 
from [dbo].[NL_LoanFees] f inner join [dbo].[NL_LoanFeeTypes] ft on f.[LoanFeeTypeID]= ft.[LoanFeeTypeID]
where  f.LoanID = @LoanID;

select * from NL_Loans l where l.LoanID = @LoanID;

-- make cancelled payment active payment again
--update [dbo].[NL_Payments] set [PaymentStatusID] = 2, [DeletionTime]=null,[DeletedByUserID]=null where [PaymentStatusID] <> 2 and LoanID = @LoanID;