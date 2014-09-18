IF OBJECT_ID('RptMarketingChannelsSummary_CountCustomersOnSteps') IS NULL
	EXECUTE('CREATE PROCEDURE RptMarketingChannelsSummary_CountCustomersOnSteps AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptMarketingChannelsSummary_CountCustomersOnSteps
@RowType NVARCHAR(64),
@DateStart DATETIME,
@DateEnd DATETIME,
@Steps IntList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'StartRegistration' AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		COUNT(*) AS Counter
	FROM
		Customer c
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID
END
GO
