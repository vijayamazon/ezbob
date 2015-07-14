IF NOT EXISTS (SELECT * FROM LoanTransactionMethod WHERE Name='Debit Card Manual')
BEGIN
	INSERT INTO LoanTransactionMethod	(Id, Name, DisplaySort) VALUES	((SELECT COUNT(Id) FROM LoanTransactionMethod )+1, 'Debit Card Manual', 1)
END	
GO

UPDATE LoanTransactionMethod SET DisplaySort = 2 WHERE Name = 'Bank transfer'
GO
UPDATE LoanTransactionMethod SET DisplaySort = 3 WHERE Name = 'Non-Cash'
GO
UPDATE LoanTransactionMethod SET DisplaySort = 4 WHERE Name = 'Write Off'
GO
UPDATE LoanTransactionMethod SET DisplaySort = 5 WHERE Name = 'Cheque'
GO
UPDATE LoanTransactionMethod SET DisplaySort = 6 WHERE Name = 'Other'
GO
UPDATE LoanTransactionMethod SET DisplaySort = 0 WHERE Name = 'CustomerAuto'
GO
UPDATE LoanTransactionMethod SET DisplaySort = 0 WHERE Name = 'Cash'
GO
