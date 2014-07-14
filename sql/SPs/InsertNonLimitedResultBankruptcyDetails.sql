IF OBJECT_ID('InsertNonLimitedResultBankruptcyDetails') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultBankruptcyDetails AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultBankruptcyDetails
	(@ExperianNonLimitedResultId INT,
	 @BankruptcyName NVARCHAR(75),
	 @BankruptcyAddr1 NVARCHAR(30),
	 @BankruptcyAddr2 NVARCHAR(30),
	 @BankruptcyAddr3 NVARCHAR(30),
	 @BankruptcyAddr4 NVARCHAR(30),
	 @BankruptcyAddr5 NVARCHAR(30),
	 @PostCode NVARCHAR(8),
	 @GazetteDate DATETIME,
	 @BankruptcyType NVARCHAR(3),
	 @BankruptcyTypeDesc NVARCHAR(20))
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultBankruptcyDetails
		(ExperianNonLimitedResultId, 
		 BankruptcyName,
		 BankruptcyAddr1,
		 BankruptcyAddr2,
		 BankruptcyAddr3,
		 BankruptcyAddr4,
		 BankruptcyAddr5,
		 PostCode,
		 GazetteDate,
		 BankruptcyType,
		 BankruptcyTypeDesc)
	VALUES
		(@ExperianNonLimitedResultId, 
		 @BankruptcyName,
		 @BankruptcyAddr1,
		 @BankruptcyAddr2,
		 @BankruptcyAddr3,
		 @BankruptcyAddr4,
		 @BankruptcyAddr5,
		 @PostCode,
		 @GazetteDate,
		 @BankruptcyType,
		 @BankruptcyTypeDesc)
END
GO
