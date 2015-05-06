
IF OBJECT_ID('SalesForceLog') IS NULL
BEGIN
	CREATE TABLE SalesForceLog(
		SalesForceLogID INT NOT NULL IDENTITY(1,1),
		Created DATETIME NOT NULL,
		CustomerID INT,
		Type NVARCHAR(30),
		Model NVARCHAR(MAX),
		Error NVARCHAR(1000),
		CONSTRAINT PK_SalesForceID PRIMARY KEY (SalesForceLogID)
	)
END
GO
