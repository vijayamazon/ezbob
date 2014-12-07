-- Find relevant customers
SELECT DISTINCT
	l.CustomerId AS CustomerID
INTO
	#c
FROM
	Loan l
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0

-------------------------------------------------------------------------------

-- Select login events only
SELECT
	LEFT(CONVERT(NVARCHAR(MAX), ue.EventArguments), 72) EventArguments,
	ue.EventTime
INTO
	#lie
FROM
	UiEvents ue
	INNER JOIN UiControls uc
		ON ue.UiControlID = uc.UiControlID
		AND uc.UiControlName = 'login:user_name'
	INNER JOIN UiActions ua
		ON ue.UiActionID = ua.UiActionID
		AND ua.UiActionName = 'focusout'
WHERE
	ue.EventArguments IS NOT NULL

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

-- Registration time
SELECT
	CustomerID = c.Id,
	EventName = 'Registration time',
	EventTime = c.GreetingMailSentDate
FROM
	Customer c
	INNER JOIN #c ON c.Id = #c.CustomerID

-------------------------------------------------------------------------------
UNION
-- Entered personal details
SELECT
	CustomerID = c.Id,
	EventName = 'Entered personal details',
	EventTime = ue.EventTime
FROM
	Customer c
	INNER JOIN #c ON c.Id = #c.CustomerID
	INNER JOIN UiEvents ue ON c.Id = ue.UserID
	INNER JOIN UiControls uc
		ON ue.UiControlID = uc.UiControlID
		AND uc.UiControlName = 'personal-info:continue' 

-------------------------------------------------------------------------------
UNION
-- Entered company details
SELECT
	CustomerID = c.Id,
	EventName = 'Entered company details',
	EventTime = ue.EventTime
FROM
	Customer c
	INNER JOIN #c ON c.Id = #c.CustomerID
	INNER JOIN UiEvents ue ON c.Id = ue.UserID
	INNER JOIN UiControls uc
		ON ue.UiControlID = uc.UiControlID
		AND uc.UiControlName = 'personal-info:company_continue' 

-------------------------------------------------------------------------------
UNION
-- Marketplace linked
SELECT
	CustomerID = c.Id,
	EventName = 'Marketplace linked',
	EventTime = m.Created
FROM
	Customer c
	INNER JOIN #c ON c.Id = #c.CustomerID
	INNER JOIN MP_CustomerMarketPlace m ON c.Id = m.CustomerId

-------------------------------------------------------------------------------
UNION
-- Application complete
SELECT
	CustomerID = c.Id,
	EventName = 'Application complete',
	EventTime = MIN(r.CreationDate)
FROM
	Customer c
	INNER JOIN #c ON c.Id = #c.CustomerID
	INNER JOIN CashRequests r ON c.Id = r.IdCustomer
GROUP BY
	c.Id

-------------------------------------------------------------------------------
UNION
-- Logged in
SELECT
	CustomerID = c.Id,
	EventName = 'Logged in',
	EventTime = ue.EventTime
FROM
	Customer c
	INNER JOIN #c ON c.Id = #c.CustomerID
	INNER JOIN #lie ue
		ON ue.EventArguments LIKE c.Name

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

DROP TABLE #lie
DROP TABLE #c
