IF OBJECT_ID('GetNonLimitedBankruptcyDetails') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedBankruptcyDetails AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedBankruptcyDetails
	(@ExperianNonLimitedResultId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		BankruptcyName,
		BankruptcyAddr1,
		BankruptcyAddr2,
		BankruptcyAddr3,
		BankruptcyAddr4,
		BankruptcyAddr5,
		PostCode,
		GazetteDate,
		BankruptcyType,
		BankruptcyTypeDesc
	FROM 
		ExperianNonLimitedResultBankruptcyDetails 
	WHERE 
		ExperianNonLimitedResultId = @ExperianNonLimitedResultId
END
GO
