GO
ALTER TABLE dbo.Customer ADD
	FirstLoanDate DateTime NULL 
GO

ALTER TABLE dbo.Customer ADD
	LastLoanDate DateTime NULL
GO

ALTER TABLE dbo.Customer ADD
	AmountTaken decimal(18, 4) NOT NULL CONSTRAINT DF_Customer_AmountTaken DEFAULT 0
GO

ALTER TABLE dbo.Customer ADD
	LastLoanAmount decimal(18, 4) NOT NULL CONSTRAINT DF_Customer_LastLoanAmount DEFAULT 0
GO

-- --------------------
ALTER TABLE dbo.Customer SET (LOCK_ESCALATION = TABLE)
GO

-- --------------------

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
SET Customer.AmountTaken=t.AmountTaken
from
(
  SELECT SUM(LoanAmount) 'AmountTaken', CustomerId
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