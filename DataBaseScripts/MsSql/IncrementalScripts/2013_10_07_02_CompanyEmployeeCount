IF OBJECT_ID('CompanyEmployeeCount') IS NULL
	CREATE TABLE CompanyEmployeeCount (
		Id INT IDENTITY(1, 1) NOT NULL,
		CustomerId INT NOT NULL,
		Created DATETIME NOT NULL CONSTRAINT DF_CompanyEmployeeCount_Created DEFAULT(GETDATE()),
		EmployeeCount INT NOT NULL,
		TopEarningEmployeeCount INT NOT NULL,
		BottomEarningEmployeeCount INT NOT NULL,
		EmployeeCountChange INT NOT NULL,
		CONSTRAINT PK_CompanyEmployeeCount PRIMARY KEY (Id),
		CONSTRAINT FK_CompanyEmployeeCount_Customer FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
	)
GO
