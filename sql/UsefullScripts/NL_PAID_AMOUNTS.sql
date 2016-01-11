DECLARE @LoanID bigint;
set @LoanID = 14;

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

select l.*, st.LoanStatus from NL_Loans l join NL_LoanStatuses st on st.LoanStatusID=l.LoanStatusID where l.LoanID = @LoanID;
select * from LoanTransaction where LoanId = (select OldLoanID from NL_Loans l where l.LoanID = @LoanID);

--select * from NL_LoanRollovers r join NL_LoanHistory h on h.LoanHistoryID=r.LoanHistoryID where h.LoanID=@LoanID;

-- rollovers
select --r.Id as rollID, r.Created, r.CreatorName, r.CustomerConfirmationDate, r.ExpiryDate, r.LoanScheduleId, r.MounthCount, r.PaidPaymentAmount, r.Payment, r.Status as rolloStatus,
r.*, s.*,
--s.Id as ScheduleID, s.AmountDue, s.Date as ScheduleDate, s.LoanId,
l.Date as loanIssueDate, l.CustomerId,  l.RefNum,  nl.LoanID
 from PaymentRollover r join LoanSchedule s on s.Id=r.LoanScheduleId join Loan l on l.Id=s.LoanId left join NL_Loans nl on nl.OldLoanID=l.Id
 where l.Id= (select OldLoanID from NL_Loans where LoanID=@LoanID)
 order by r.Id desc;
 select * from NL_LoanRollovers r join NL_LoanHistory h on h.LoanHistoryID=r.LoanHistoryID and h.LoanID=@LoanID;

 
-- make cancelled payment active payment again
--update [dbo].[NL_Payments] set [PaymentStatusID] = 2, [DeletionTime]=null,[DeletedByUserID]=null where [PaymentStatusID] <> 2 and LoanID = @LoanID;

--update NL_LoanSchedules set LoanScheduleStatusID=1, ClosedTime=null  where LoanScheduleID=84
