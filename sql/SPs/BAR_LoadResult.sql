SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_LoadResult') IS NULL
	EXECUTE('CREATE PROCEDURE BAR_LoadResult AS SELECT 1')
GO

ALTER PROCEDURE BAR_LoadResult
@TrailTagID BIGINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF @TrailTagID IS NULL
	BEGIN
		SELECT
			@TrailTagID = MAX(TrailTagID)
		FROM
			DecisionTrailTags
		WHERE
			TrailTag LIKE '#BravoAutoRpt%'
	END

	SELECT
		bar.HasEnoughData,
		bar.IsOldCustomer,
		bar.HasSignature,
		ManualDecision = md.DecisionName,
		ActualAuto = aa.DecisionName,
		PossibleAuto = pa.DecisionName
	FROM
		BAR_Results bar
		INNER JOIN CashRequests r ON bar.FirstCashRequestID = r.Id
		INNER JOIN Decisions md ON bar.ManualDecisionID = md.DecisionID
		INNER JOIN Decisions pa ON bar.AutoDecisionID = pa.DecisionID
		LEFT JOIN Decisions aa ON r.AutoDecisionID = aa.DecisionID
	WHERE
		bar.TrailTagID = @TrailTagID
	ORDER BY
		bar.HasEnoughData,
		bar.IsOldCustomer
END
GO
