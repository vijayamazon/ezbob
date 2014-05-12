IF OBJECT_ID('CciHistory') IS NULL
BEGIN
	CREATE TABLE CciHistory
	(
		Id INT IDENTITY,
		CustomerId INT NOT NULL,
		ChangeDate DATETIME NOT NULL,
		CciMark BIT NOT NULL
	)
	
END
GO
