SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BAR_LoadCashRequests') IS NULL
	EXECUTE('CREATE PROCEDURE BAR_LoadCashRequests AS SELECT 1')
GO

ALTER PROCEDURE BAR_LoadCashRequests
@StartTime DATETIME = NULL,
@EndTime DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	;WITH loans AS (
		SELECT
			CustomerID = c.Id,
			LoanID = l.Id,
			IssueTime = l.[Date]
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	), signatures AS (
		SELECT DISTINCT
			docs.CustomerID,
			docs.SendDate,
			usr.SignDate
		FROM
			Esignatures docs
			INNER JOIN Esigners usr ON docs.EsignatureID = usr.EsignatureID
			INNER JOIN Customer c ON docs.CustomerID = c.Id AND c.IsTest = 0
		WHERE
			usr.SignDate IS NOT NULL
	), aml_checks AS (
		SELECT DISTINCT
			CustomerID = c.Id,
			CheckTime = l.InsertDate,
			l.Id
		FROM
			MP_ServiceLog l
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			l.ServiceType = 'AML A check'
	), consumer_checks AS (
		SELECT DISTINCT
			CustomerID = c.Id,
			CheckTime = l.InsertDate,
			l.Id
		FROM
			MP_ServiceLog l
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		WHERE
			l.ServiceType = 'Consumer Request'
			AND
			l.DirectorId IS NULL
	), company_checks AS (
		SELECT DISTINCT
			CustomerID = c.Id,
			CheckTime = l.InsertDate,
			l.Id
		FROM
			MP_ServiceLog l
			INNER JOIN Company co ON l.CompanyRefNum = co.ExperianRefNum
			INNER JOIN Customer c ON c.CompanyId = co.Id AND c.IsTest = 0
		WHERE
			l.ServiceType IN ('E-SeriesLimitedData', 'E-SeriesNonLimitedData')
	), mp_update_raw AS (
		SELECT CustomerID = c.Id, MpID = m.Id, h.UpdatingStart, h.UpdatingEnd
		FROM MP_CustomerMarketPlaceUpdatingHistory h
		INNER JOIN MP_CustomerMarketPlace m ON h.CustomerMarketPlaceId = m.Id AND ISNULL(m.Disabled, 0) = 0
		INNER JOIN Customer c ON m.CustomerId = c.Id AND c.IsTest = 0
		WHERE h.UpdatingStart IS NOT NULL
		AND h.UpdatingEnd IS NOT NULL
	)
	SELECT
		CashRequestID = r.Id,
		CustomerID = r.IdCustomer,
		IsApproved = CONVERT(BIT, CASE r.UnderwriterDecision WHEN 'Rejected' THEN 0 ELSE 1 END),	
		r.AutoDecisionID,
		DecisionTime = r.UnderwriterDecisionDate,
		IsAlibaba = CONVERT(BIT, CASE WHEN c.AlibabaId IS NULL THEN 0 ELSE 1 END),
		IsOldCustomer = CONVERT(BIT, CASE WHEN EXISTS (
			SELECT COUNT(*)
			FROM loans l
			WHERE l.CustomerID = c.Id
			AND l.IssueTime < r.UnderwriterDecisionDate
			HAVING COUNT(*) > 0
		) THEN 1 ELSE 0 END),
		HasSignature = CONVERT(BIT, CASE WHEN EXISTS (
			SELECT *
			FROM signatures s
			WHERE s.CustomerID = c.Id
			AND s.SendDate BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
			AND s.SignDate BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
		) THEN 1 ELSE 0 END),
		AmlChecked = CONVERT(BIT, CASE WHEN EXISTS (
			SELECT COUNT(*)
			FROM aml_checks ch
			WHERE ch.CustomerID = c.Id
			AND ch.CheckTime BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
			HAVING COUNT(*) > 1
		) THEN 1 ELSE 0 END),
		ConsumerChecked = CONVERT(BIT, CASE WHEN EXISTS (
			SELECT COUNT(*)
			FROM consumer_checks ch
			WHERE ch.CustomerID = c.Id
			AND ch.CheckTime BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
			HAVING COUNT(*) > 1
		) THEN 1 ELSE 0 END),
		CompanyChecked = CONVERT(BIT, CASE WHEN EXISTS (
			SELECT COUNT(*)
			FROM company_checks ch
			WHERE ch.CustomerID = c.Id
			AND ch.CheckTime BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
			HAVING COUNT(*) > 1
		) THEN 1 ELSE 0 END),
		MpUpdated = CONVERT(BIT, CASE WHEN EXISTS (
			SELECT u.MpID, COUNT(*)
			FROM mp_update_raw u
			WHERE u.CustomerID = c.Id
			AND u.UpdatingStart BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
			AND u.UpdatingEnd BETWEEN r.CreationDate AND r.UnderwriterDecisionDate
			GROUP BY u.MpID
			HAVING COUNT(*) > 1
		) THEN 1 ELSE 0 END)
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		r.CreationDate > ISNULL(@StartTime, 'May 11 2015') -- on May 11 2015 we have released Auto Approve (the last unreleased auto decision)
		AND
		(@EndTime IS NULL OR r.CreationDate < @EndTime)
		AND
		r.UnderwriterDecision IN ('Approved', 'ApprovedPending', 'Rejected')
	ORDER BY
		r.IdCustomer,
		r.UnderwriterDecisionDate
END
GO
