IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsWasLate' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	alter table Customer add IsWasLate bit null
END
go

-- set is was late 
UPDATE Customer SET IsWasLate = 1
FROM  
(
SELECT  distinct(l.CustomerId) 'CustomerId'
FROM LoanSchedule ls LEFT JOIN Loan l ON ls.LoanId = l.Id 
WHERE  ls.[Status] = 'Paid'
) t
where Customer.Id=t.CustomerId

-- set is not was late

UPDATE Customer SET IsWasLate = 0
FROM  
(
SELECT  
	distinct(l.CustomerId) 'CustomerId',
	CASE 
	when  (SELECT COUNT(*)  
			FROM LoanSchedule _ls, loan _l, Customer _c 
			WHERE _ls.LoanId = _l.Id AND _l.CustomerId = _c.Id AND _ls.[Status] = 'Paid' AND l.CustomerId = _c.Id) = 0 
	THEN 1
	END AS 'lateCount'
FROM LoanSchedule ls LEFT JOIN Loan l 
ON ls.LoanId = l.Id 
) t
where Customer.Id=t.CustomerId AND t.lateCount IS NOT null
