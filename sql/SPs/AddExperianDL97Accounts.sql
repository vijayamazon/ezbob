IF OBJECT_ID('AddExperianDL97Accounts') IS NULL
	EXECUTE('CREATE PROCEDURE AddExperianDL97Accounts AS SELECT 1')
GO

ALTER PROCEDURE AddExperianDL97Accounts
	(@CustomerId INT,
	 @State CHAR(1),
	 @Type VARCHAR(2),
	 @Status12Months VARCHAR(12),
	 @LastUpdated DATETIME,
	 @CompanyType VARCHAR(2),
	 @CurrentBalance INT,
	 @MonthsData INT,
	 @Status1To2 INT,
	 @Status3To9 INT)
AS
BEGIN
	INSERT INTO ExperianDL97Accounts
		(CustomerId, State, Type, Status12Months, LastUpdated, CompanyType, CurrentBalance, MonthsData, Status1To2, Status3To9)
	VALUES
		(@CustomerId, @State, @Type, @Status12Months, @LastUpdated, @CompanyType, @CurrentBalance, @MonthsData, @Status1To2, @Status3To9)
END
GO
