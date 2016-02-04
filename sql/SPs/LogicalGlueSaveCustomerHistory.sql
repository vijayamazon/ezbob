SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('LogicalGlueSaveCustomerHistory') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueSaveCustomerHistory AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueSaveCustomerHistory
@ResponseID BIGINT,
@Now DATETIME
AS
BEGIN
	DECLARE @ModelID BIGINT = (SELECT m.ModelID FROM LogicalGlueModels m WHERE m.ModelName = 'Neural network')

	------------------------------------------------------------------------------

	;WITH lg AS (
		SELECT
			l.CustomerID,
			l.CompanyID,
			rs.ResponseID,
			rs.GradeID,
			mo.Score,
			etl.EtlCodeID,
			etlc.IsHardReject,
			rs.ErrorMessage,
			rs.ParsingExceptionType,
			rs.ParsingExceptionMessage,
			rs.TimeoutSourceID,
			mo.ModelOutputID,
			mo.ErrorCode,
			mo.Exception,
			EncodingFailureCount = (SELECT COUNT(*) FROM LogicalGlueModelEncodingFailures WHERE ModelOutputID = mo.ModelOutputID),
			MissingColumnCount = (SELECT COUNT(*) FROM LogicalGlueModelMissingColumns WHERE ModelOutputID = mo.ModelOutputID),
			WarningCount = (SELECT COUNT(*) FROM LogicalGlueModelWarnings WHERE ModelOutputID = mo.ModelOutputID)
		FROM
			LogicalGlueResponses rs
			INNER JOIN MP_ServiceLog l ON l.Id = rs.ServiceLogID
			LEFT JOIN LogicalGlueEtlData etl ON etl.ResponseID = rs.ResponseID
			LEFT JOIN LogicalGlueModelOutputs mo ON rs.ResponseID = mo.ResponseID AND mo.ModelID = @ModelID
			LEFT JOIN LogicalGlueEtlCodes etlc ON etl.EtlCodeID = etlc.EtlCodeID
		WHERE
			rs.ResponseID = @ResponseID
	), lg_one AS (
		SELECT
			CustomerID,
			CompanyID,
			ResponseID,
			GradeID,
			Score,
			IsHardReject = ISNULL(IsHardReject, 0),
			HasModel = CONVERT(BIT, CASE WHEN ModelOutputID IS NULL THEN 0 ELSE 1 END),
			HasModelError = CONVERT(BIT, CASE
				WHEN
					LTRIM(RTRIM(ISNULL(ErrorCode, ''))) = '' AND
					LTRIM(RTRIM(ISNULL(Exception, ''))) = '' AND
					EncodingFailureCount = 0 AND
					MissingColumnCount = 0 AND
					WarningCount = 0
				THEN
					0
				ELSE
					1
			END),
			HasResponseError = CONVERT(BIT, CASE
				WHEN
					EtlCodeID IS NOT NULL AND
					LTRIM(RTRIM(ISNULL(ErrorMessage, ''))) = '' AND
					LTRIM(RTRIM(ISNULL(ParsingExceptionType, ''))) = '' AND
					LTRIM(RTRIM(ISNULL(ParsingExceptionMessage, ''))) = '' AND
					TimeoutSourceID IS NULL
				THEN
					0
				ELSE
					1
			END)
		FROM
			lg
	) SELECT
		CustomerID,
		CompanyID,
		ResponseID,
		GradeID,
		Score,
		IsHardReject,
		ScoreIsReliable = CONVERT(BIT, CASE WHEN HasModel = 0 OR HasModelError = 0 THEN 1 ELSE 0 END),
		ErrorInResponse = HasResponseError
	INTO
		#lg
	FROM
		lg_one

	------------------------------------------------------------------------------

	UPDATE CustomerLogicalGlueHistory SET
		IsActive = 0
	FROM
		CustomerLogicalGlueHistory h
		INNER JOIN #lg l
			ON h.CustomerID = l.CustomerID
			AND h.CompanyID = l.CompanyID

	------------------------------------------------------------------------------

	INSERT INTO CustomerLogicalGlueHistory (
		CustomerID, CompanyID, ResponseID, IsActive, SetTime,
		GradeID, Score, IsHardReject, ScoreIsReliable, ErrorInResponse
	) SELECT
		CustomerID, CompanyID, ResponseID, 1, @Now,
		GradeID, Score, IsHardReject, ScoreIsReliable, ErrorInResponse
	FROM
		#lg

	------------------------------------------------------------------------------

	DROP TABLE #lg
END
GO
