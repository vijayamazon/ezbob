IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'DeletedDate' AND Object_ID = Object_ID(N'LoanCharges'))
BEGIN
    ALTER TABLE LoanCharges ADD DeletedDate datetime NULL
	ALTER TABLE LoanCharges ADD UserID INT NULL
END
