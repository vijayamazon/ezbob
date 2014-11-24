IF NOT EXISTS (SELECT * FROM LoanTransactionMethod WHERE Name='CustomerAuto')
BEGIN
	INSERT INTO dbo.LoanTransactionMethod	(Id, Name, DisplaySort) VALUES	(9, 'CustomerAuto', 6)
END	
	
GO