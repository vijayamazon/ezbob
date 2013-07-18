IF OBJECT_ID ('dbo.PacNetManualBalance') IS NULL
	CREATE TABLE dbo.PacNetManualBalance
	(
		Id INT IDENTITY NOT NULL,
		Username VARCHAR(100),
		Amount INT NOT NULL,
		Date DateTime,
		Enabled BIT DEFAULT 1
	)  
GO
