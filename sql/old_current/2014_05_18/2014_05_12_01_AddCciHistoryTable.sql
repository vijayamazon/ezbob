IF OBJECT_ID('CciHistory') IS NULL
BEGIN
	CREATE TABLE CciHistory
	(
		Id INT IDENTITY,
		Username VARCHAR(100),
		CustomerId INT NOT NULL,
		ChangeDate DATETIME NOT NULL,
		CciMark BIT NOT NULL
	)
	
END
GO
