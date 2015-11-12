SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadResetCashRequests') IS NULL
	EXECUTE('CREATE PROCEDURE LoadResetCashRequests AS SELECT 1')
GO

ALTER PROCEDURE LoadResetCashRequests
@TimeSlice DATETIME
AS
BEGIN
	SELECT
		CashRequestID = h.CashRequestId,
		DecisionName = h.Action,
		DecisionTime = h.[Date],
		CustomerID = h.CustomerId,
		CustomerName = c.Fullname,
		CustomerEmail = c.Name,
		UnderwriterID = h.UnderwriterId,
		UnderwriterName = u.UserName
	FROM
		CashRequests r
		INNER JOIN DecisionHistory h ON r.Id = h.CashRequestId
		INNER JOIN Customer c ON h.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN Security_User u ON h.UnderwriterId = u.UserId
	WHERE
		h.[Date] > @TimeSlice
		AND
		r.UnderwriterDecision IS NULL
		AND
		h.Action IN ('Approve', 'Reject', 'ReReject', 'ReApprove')
	ORDER BY
		h.CustomerId,
		h.[Date]
END
GO
