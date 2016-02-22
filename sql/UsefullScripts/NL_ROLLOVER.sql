declare @oldLoanID int;
set @oldLoanID=5120;

select --r.Id as rollID, r.Created, r.CreatorName, r.CustomerConfirmationDate, r.ExpiryDate, r.LoanScheduleId, r.MounthCount, r.PaidPaymentAmount, r.Payment, r.Status as rolloStatus,
r.*, s.*,
--s.Id as ScheduleID, s.AmountDue, s.Date as ScheduleDate, s.LoanId,
l.Date as loanIssueDate, l.CustomerId,  l.RefNum,  nl.LoanID
 from PaymentRollover r join LoanSchedule s on s.Id=r.LoanScheduleId join Loan l on l.Id=s.LoanId left join NL_Loans nl on nl.OldLoanID=l.Id
 where l.Id= @oldLoanID
 order by r.Id desc;

 select * from NL_LoanRollovers r join NL_LoanHistory h on h.LoanHistoryID=r.LoanHistoryID and h.LoanID=(select LoanID from NL_Loans where OldLoanID=@oldLoanID);