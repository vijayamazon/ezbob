IF OBJECT_ID('InsertNonLimitedResultPreviousSearches') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultPreviousSearches AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultPreviousSearches
	(@ExperianNonLimitedResultId INT,
	 @PreviousSearchDate DATETIME,
	 @EnquiryType NVARCHAR(1),	
	 @EnquiryTypeDesc NVARCHAR(50),
	 @CreditRequired NVARCHAR(13))
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultPreviousSearches
		(ExperianNonLimitedResultId,
		 PreviousSearchDate,
		 EnquiryType,	
		 EnquiryTypeDesc,
		 CreditRequired)
	VALUES
		(@ExperianNonLimitedResultId,
		 @PreviousSearchDate,
		 @EnquiryType,	
		 @EnquiryTypeDesc,
		 @CreditRequired)
END
GO
