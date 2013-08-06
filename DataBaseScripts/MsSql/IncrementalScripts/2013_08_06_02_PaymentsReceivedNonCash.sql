IF NOT EXISTS (
	SELECT
		*
	FROM
		ReportArguments a
		INNER JOIN ReportScheduler r ON a.ReportId = r.Id
		INNER JOIN ReportArgumentNames n ON a.ReportArgumentNameId = n.Id
	WHERE
		n.Name = 'ShowNonCashTransactions'
		AND
		r.Type = 'RPT_PAYMENTS_RECEIVED'
)
BEGIN
	INSERT INTO ReportArguments (ReportArgumentNameId, ReportId)
	SELECT
		n.Id,
		r.Id
	FROM
		ReportScheduler r
		INNER JOIN ReportArgumentNames n ON n.Name = 'ShowNonCashTransactions'
	WHERE
		r.Type = 'RPT_PAYMENTS_RECEIVED'
END
GO