GO
ALTER TABLE dbo.Customer ADD
	TotalPrincipalRepaid decimal(18, 4) NOT NULL CONSTRAINT DF_Customer_TotalPrincipalRepaid DEFAULT 0
GO

UPDATE Customer
SET Customer.TotalPrincipalRepaid=tt.[TotalPrincipal]
FROM  
(
	SELECT sum(t.[LoanRepayment]) 'TotalPrincipal', l.CustomerId from [LoanTransaction] t left join [Loan] l
        on t.[LoanId] = l.[Id]
        where t.[Type] = 'PaypointTransaction'  and  t.[Status] != 'Error'
 GROUP BY l.CustomerId
) tt
where Customer.Id=tt.CustomerId
GO
