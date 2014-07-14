IF OBJECT_ID('InsertNonLimitedResultCcjDetails') IS NULL
	EXECUTE('CREATE PROCEDURE InsertNonLimitedResultCcjDetails AS SELECT 1')
GO

ALTER PROCEDURE InsertNonLimitedResultCcjDetails
	(@ExperianNonLimitedResultId INT,		
	 @RecordType NVARCHAR(1),
	 @RecordTypeFullName NVARCHAR(10),
	 @JudgementDate DATETIME,
	 @SatisfactionFlag NVARCHAR(1),
	 @SatisfactionFlagDesc NVARCHAR(15),
	 @SatisfactionDate DATETIME,
	 @JudgmentType NVARCHAR(3),
	 @JudgmentTypeDesc NVARCHAR(35),
	 @JudgmentAmount INT,
	 @Court NVARCHAR(30),
	 @CaseNumber NVARCHAR(11),
	 @NumberOfJudgmentNames NVARCHAR(2),
	 @NumberOfTradingNames NVARCHAR(2),
	 @LengthOfJudgmentName NVARCHAR(3),
	 @LengthOfTradingName NVARCHAR(3),
	 @LengthOfJudgmentAddress NVARCHAR(3),
	 @JudgementAddr1 NVARCHAR(30),
	 @JudgementAddr2 NVARCHAR(30),
	 @JudgementAddr3 NVARCHAR(30),
	 @JudgementAddr4 NVARCHAR(30),
	 @JudgementAddr5 NVARCHAR(30),
	 @PostCode NVARCHAR(8))
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO ExperianNonLimitedResultCcjDetails
		(ExperianNonLimitedResultId,		
		 RecordType,
		 RecordTypeFullName,
		 JudgementDate,
		 SatisfactionFlag,
		 SatisfactionFlagDesc,
		 SatisfactionDate,
		 JudgmentType,
		 JudgmentTypeDesc,
		 JudgmentAmount,
		 Court,
		 CaseNumber,
		 NumberOfJudgmentNames,
		 NumberOfTradingNames,
		 LengthOfJudgmentName,
		 LengthOfTradingName,
		 LengthOfJudgmentAddress,
		 JudgementAddr1,
		 JudgementAddr2,
		 JudgementAddr3,
		 JudgementAddr4,
		 JudgementAddr5,
		 PostCode)
	VALUES
		(@ExperianNonLimitedResultId,		
		 @RecordType,
		 @RecordTypeFullName,
		 @JudgementDate,
		 @SatisfactionFlag,
		 @SatisfactionFlagDesc,
		 @SatisfactionDate,
		 @JudgmentType,
		 @JudgmentTypeDesc,
		 @JudgmentAmount,
		 @Court,
		 @CaseNumber,
		 @NumberOfJudgmentNames,
		 @NumberOfTradingNames,
		 @LengthOfJudgmentName,
		 @LengthOfTradingName,
		 @LengthOfJudgmentAddress,
		 @JudgementAddr1,
		 @JudgementAddr2,
		 @JudgementAddr3,
		 @JudgementAddr4,
		 @JudgementAddr5,
		 @PostCode)
		
	SELECT @@IDENTITY AS NewId
END
GO
