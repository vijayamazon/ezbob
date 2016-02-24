SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_LogicalGlueDataForAutoReject') IS NULL
	EXECUTE('CREATE PROCEDURE AV_LogicalGlueDataForAutoReject AS SELECT 1')
GO

ALTER PROCEDURE AV_LogicalGlueDataForAutoReject
@CustomerID INT,
@CompanyID INT,
@ProcessingDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ModelID BIGINT = (SELECT m.ModelID FROM LogicalGlueModels m WHERE m.ModelName = 'Neural network')

	DECLARE @OriginID INT = (SELECT c.OriginID FROM Customer c WHERE c.Id = @CustomerID)

	DECLARE @LoanSourceID INT = (
		SELECT dls.LoanSourceID
		FROM DefaultLoanSources dls
		INNER JOIN Customer c
			ON dls.OriginID = c.OriginID AND c.Id = @CustomerID
	)

	;WITH meta AS (
		SELECT
			CustomerOrigin = (SELECT o.Name FROM CustomerOrigin o WHERE o.CustomerOriginID = @OriginID),
			TypeOfBusiness = (SELECT TypeOfBusiness FROM Company WHERE Id = @CompanyID),
			LoanCount = (SELECT COUNT(DISTINCT Id) FROM Loan l WHERE l.CustomerId = @CustomerID AND l.[Date] <= @ProcessingDate),
			LoanSource = (SELECT ls.LoanSourceName FROM LoanSource ls WHERE ls.LoanSourceID = @LoanSourceID)
	), more_meta AS (
		SELECT
			IsRegulated = (SELECT IsRegulated FROM TypeOfBusiness WHERE Name = m.TypeOfBusiness)
		FROM
			meta m
	), even_more_meta AS (
		SELECT
			AutoDecisionInternalLogic = ISNULL((
				SELECT AutoDecisionInternalLogic
				FROM I_ProductSubType
				WHERE OriginID = @OriginID
				AND LoanSourceID = @LoanSourceID
				AND IsRegulated = mm.IsRegulated
			), 1)
		FROM
			more_meta mm
	), lg AS (
		SELECT
			l.Id as ServiceLogID,
			RequestID = r.UniqueID,
			rs.ResponseID,
			rs.HttpStatus,
			rs.ErrorMessage,
			rs.ParsingExceptionType,
			rs.ParsingExceptionMessage, 
			Grade = g.Name,
			rs.TimeoutSourceID,
			etl.EtlCodeID,
			mo.ModelOutputID,
			mo.Score,
			mo.ErrorCode,
			mo.Exception,
			EncodingFailureCount = (SELECT COUNT(*) FROM LogicalGlueModelEncodingFailures WHERE ModelOutputID = mo.ModelOutputID),
			MissingColumnCount = (SELECT COUNT(*) FROM LogicalGlueModelMissingColumns WHERE ModelOutputID = mo.ModelOutputID),
			WarningCount = (SELECT COUNT(*) FROM LogicalGlueModelWarnings WHERE ModelOutputID = mo.ModelOutputID)
		FROM
			MP_ServiceLog l
			INNER JOIN LogicalGlueRequests r
				ON l.Id = r.ServiceLogID	 
				AND r.ServiceLogID = (
					SELECT TOP 1 
						r1.ServiceLogID
					FROM LogicalGlueRequests r1
					INNER JOIN MP_ServiceLog l1
						ON l1.Id = r1.ServiceLogID
						AND l1.CustomerId = @CustomerID
						AND l1.CompanyID = @CompanyID 
						AND l1.ServiceType = 'LogicalGlue'
						AND r1.IsTryOut = 0
						AND l1.InsertDate <= @ProcessingDate
					ORDER BY
						l1.InsertDate DESC,
						l1.Id DESC
				)
			LEFT JOIN LogicalGlueResponses rs ON rs.ServiceLogID = l.Id 
			LEFT JOIN LogicalGlueEtlData etl ON etl.ResponseID = rs.ResponseID
			LEFT JOIN LogicalGlueModelOutputs mo ON rs.ResponseID = mo.ResponseID AND mo.ModelID = @ModelID
			LEFT JOIN I_Grade g ON rs.GradeID = g.GradeID
	) SELECT
		meta.CustomerOrigin,
		meta.TypeOfBusiness,
		meta.LoanCount,
		meta.LoanSource,
		mm.IsRegulated,
		mmm.AutoDecisionInternalLogic,
		lg.RequestID,
		lg.ResponseID,
		lg.HttpStatus,
		lg.ErrorMessage,
		lg.ParsingExceptionType,
		lg.ParsingExceptionMessage, 
		lg.Grade,
		lg.TimeoutSourceID,
		lg.EtlCodeID,
		lg.ModelOutputID,
		lg.Score,
		lg.ErrorCode,
		lg.Exception,
		lg.EncodingFailureCount,
		lg.MissingColumnCount
	FROM
		meta
		FULL OUTER JOIN more_meta mm ON 1 = 1
		FULL OUTER JOIN even_more_meta mmm ON 1 = 1
		FULL OUTER JOIN lg ON 1 = 1
END
GO
