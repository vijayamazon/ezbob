-- 1. Approve.
-- 2. Reject.
-- 6. Re-approve.
-- 7. Re-reject.

UPDATE CashRequests SET
	AutoDecisionID = 6
FROM
	CashRequests r
	INNER JOIN DecisionHistory h ON r.Id = h.CashRequestId
WHERE
	h.Comment LIKE '%Auto%'
	AND
	h.Comment LIKE '%Re-Approv%'
	AND
	h.UnderwriterId = 1
GO

UPDATE CashRequests SET
	AutoDecisionID = 1
FROM
	CashRequests r
	INNER JOIN DecisionHistory h ON r.Id = h.CashRequestId
WHERE
	h.Comment LIKE '%Auto%'
	AND
	h.Comment LIKE '%Approv%'
	AND
	h.Comment NOT LIKE '%Re-Approv%'
	AND
	h.UnderwriterId = 1
GO

UPDATE CashRequests SET
	AutoDecisionID = 2
FROM
	CashRequests r
	INNER JOIN DecisionHistory h ON r.Id = h.CashRequestId
WHERE
	h.Comment LIKE 'AutoReject%'
	AND
	h.UnderwriterId = 1
GO

UPDATE CashRequests SET
	AutoDecisionID = 7
FROM
	CashRequests r
	INNER JOIN DecisionHistory h ON r.Id = h.CashRequestId
WHERE
	h.Comment LIKE 'Auto Re-Reject%'
	AND
	h.UnderwriterId = 1
GO
