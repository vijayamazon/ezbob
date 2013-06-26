UPDATE Customer
SET Customer.FirstLoanDate=t.FirstLoanDate
from
(
  SELECT MIN(Date) 'FirstLoanDate', CustomerId
  FROM [Loan]
  GROUP BY CustomerId
 ) t

WHERE Customer.Id=t.CustomerId
GO

UPDATE Customer
SET Customer.LastLoanDate=t.LastLoanDate
from
(
  SELECT MAX(Date) 'LastLoanDate', CustomerId
  FROM [Loan]
  GROUP BY CustomerId
 ) t

WHERE Customer.Id=t.CustomerId
GO

UPDATE Customer
SET Customer.LastLoanAmount=d.LastLoanAmount
from (
select t.CustomerId, LoanAmount 'LastLoanAmount' from Loan,
	(
	  SELECT MAX(Id) 'Id', CustomerId
	  FROM [Loan]
	  GROUP BY CustomerId
	 ) t
	where Loan.Id = t.Id
)d
 where Customer.Id = d.CustomerId
GO