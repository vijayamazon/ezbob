IF object_id('FrequentActionItems') IS NULL
BEGIN
	CREATE TABLE FrequentActionItems(
	    Id INT NOT NULL IDENTITY(1,1)
	   ,Item NVARCHAR(1000) NOT NULL
	   ,IsActive BIT NOT NULL
	   ,CONSTRAINT PK_FrequentActionItems PRIMARY KEY (Id)
	)	
END
GO
