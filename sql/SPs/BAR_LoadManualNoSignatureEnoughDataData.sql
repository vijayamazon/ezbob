SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_LoadManualNoSignatureEnoughDataData') IS NULL
	EXECUTE('CREATE PROCEDURE BAR_LoadManualNoSignatureEnoughDataData AS SELECT 1')
GO

ALTER PROCEDURE BAR_LoadManualNoSignatureEnoughDataData
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
		br.CustomerID,
		br.FirstCashRequestID,
		UwName = u.UserName,
		DecisionTime = r.UnderwriterDecisionDate,
		br.ManualDecisionID,
		rr.Reason,
		r.UnderwriterComment,
		MpID = m.Id,
		MpType = mt.Name,
		MpTypeInternalID = mt.InternalId,
		MpTotalsMonth = dbo.udfGetLatestTotalsMonth(m.Id, r.UnderwriterDecisionDate),
		br.AutoApproveTrailID,
		t.DecisionStatusID,
		tn.TraceName
	FROM
		BAR_Results br
		INNER JOIN CashRequests r
			ON br.FirstCashRequestID = r.Id
			AND r.AutoDecisionID IS NULL
		LEFT JOIN Security_User u ON r.IdUnderwriter = u.UserId
		LEFT JOIN DecisionHistory dh ON r.Id = dh.CashRequestId
		LEFT JOIN DecisionHistoryRejectReason dhrr ON dh.Id = dhrr.DecisionHistoryId
		LEFT JOIN RejectReason rr ON dhrr.RejectReasonId = rr.Id
		LEFT JOIN MP_CustomerMarketPlace m
			ON br.CustomerID = m.CustomerId
			AND ISNULL(m.Disabled, 0) = 0
			AND m.Created < r.UnderwriterDecisionDate
		LEFT JOIN MP_MarketplaceType mt ON m.MarketPlaceId = mt.Id
		LEFT JOIN DecisionTrail t ON br.AutoApproveTrailID = t.TrailID
		LEFT JOIN DecisionTrace tc
			ON t.TrailID = tc.TrailID
			AND tc.DecisionStatusID != 1
		LEFT JOIN DecisionTraceNames tn ON tc.TraceNameID = tn.TraceNameID
	WHERE
		br.TrailTagID = @TrailTagID
		AND
		br.HasEnoughData = 1
		AND
		br.HasSignature = 0
	ORDER BY
		br.FirstCashRequestID
END
GO
