alter table LoanType add RepaymentPeriod int
go
update LoanType set RepaymentPeriod = 3 where Type = 'StandardLoanType'
go
update LoanType set RepaymentPeriod = 6 where Type = 'HalfWayLoanType'
go