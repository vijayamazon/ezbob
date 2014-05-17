IF OBJECT_ID('ExperianDL97Accounts') IS NULL
	CREATE TABLE ExperianDL97Accounts (
		Id INT IDENTITY NOT NULL,
		CustomerId INT NOT NULL,
		State CHAR(1),
		Type VARCHAR(2),
		Status12Months VARCHAR(12),
		LastUpdated DATETIME,
		CompanyType VARCHAR(2),
		CurrentBalance INT,
		MonthsData INT,
		Status1To2 INT,
		Status3To9 INT
		CONSTRAINT PK_ExperianDL97Accounts PRIMARY KEY (Id)
	)
GO

	