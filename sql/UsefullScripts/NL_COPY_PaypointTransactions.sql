
-- copy PaypointTransaction from [LoanTransaction] into NL_Payments, NL_PaypointTransactions

declare @systemUserID int =1;

Declare @ppTransactionsList table(
    OldLoanID int not null,
	TransactionID int not null,
	PostDate datetime not null,
	Amount decimal (18,6) not null,
	TransactionMethod int not null,
	[Description] nvarchar (100) null,
	IP nvarchar (60) null,
	PaypointId nvarchar (100) null,
	CardID int null,
	LoanID bigint null, 
	PaymentID bigint null 
);

-- select relevant transaction 
insert into @ppTransactionsList
select 
t.LoanId as OldLoanID,t.Id as TransactionID,t.PostDate,t.Amount,t.LoanTransactionMethodId as TransactionMethod,t.[Description],t.IP,t.PaypointId,c.Id as CardID, p.LoanID, p.PaymentID
from 
[dbo].[LoanTransaction] t inner join [dbo].[Loan] l on l.Id = t.LoanId and t.[Type] = 'PaypointTransaction'	
 left join PayPointCard c on c.TransactionId = t.PaypointId inner join NL_Loans nl on nl.OldLoanID = l.Id 
 left join [dbo].[NL_Payments] p on p.LoanID = nl.LoanID-- and p.PaymentID = null ;

select * from @ppTransactionsList;
return;

-- record NL_Payments 
insert into [dbo].[NL_Payments] (PaymentTime, Amount, CreatedByUserID, LoanID, PaymentStatusID, PaymentMethodID, CreationTime, Notes) 
SELECT PostDate,Amount,@systemUserID as CreatedByUserID,LoanID,(select s.PaymentStatusID from NL_PaymentStatuses s where s.PaymentStatus = 'Active') as PaymentStatusID,TransactionMethod,PostDate,[Description]
FROM @ppTransactionsList where PaymentID is null;

-- set newly created PaymentIDs into var table
update @ppTransactionsList set t.PaymentID = p.PaymentID
from @ppTransactionsList t inner join NL_Payments p on p.LoanID=t.LoanID and t.Amount=p.Amount and t.PostDate=p.PaymentTime and t.TransactionMethod=p.PaymentMethodID ;

-- record NL_PaypointTransactions
insert into NL_PaypointTransactions (Amount,IP,	Notes,PaypointTransactionStatusID,PaypointUniqueID,PaypointCardID,TransactionTime,PaymentID)
SELECT t.Amount, t.IP, [Description], (select s.PaypointTransactionStatusID from NL_PaypointTransactionStatuses s where s.TransactionStatus= 'Done'),PaypointId,CardID,PostDate,t.PaymentID
from  @ppTransactionsList t left join [dbo].[NL_PaypointTransactions] ppt on ppt.PaymentID=t.PaymentID where ppt.PaypointTransactionID is null