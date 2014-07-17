IF OBJECT_ID('GetNonLimitedCcjDetails') IS NULL
	EXECUTE('CREATE PROCEDURE GetNonLimitedCcjDetails AS SELECT 1')
GO

ALTER PROCEDURE GetNonLimitedCcjDetails
	(@ExperianNonLimitedResultId INT)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		Id,
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
		PostCode
	FROM 
		ExperianNonLimitedResultCcjDetails 
	WHERE 
		ExperianNonLimitedResultId = @ExperianNonLimitedResultId
END
GO
