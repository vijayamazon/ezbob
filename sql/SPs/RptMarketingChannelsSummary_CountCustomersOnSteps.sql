IF OBJECT_ID('RptMarketingChannelsSummary_CountCustomersOnSteps') IS NULL
	EXECUTE('CREATE PROCEDURE RptMarketingChannelsSummary_CountCustomersOnSteps AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptMarketingChannelsSummary_CountCustomersOnSteps
@RowType NVARCHAR(64),
@DateStart DATETIME,
@DateEnd DATETIME,
@UiControlName NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@RowType AS RowType,
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END AS ReferenceSource,
		c.GoogleCookie,
		c.BrokerID,
		COUNT(*) AS Counter
	FROM
		Customer c
		INNER JOIN UiEvents ue ON c.Id = ue.UserID
		INNER JOIN UiActions ua ON ue.UiActionID = ua.UiActionID AND ua.UiActionName = 'click'
		INNER JOIN UiControls uc ON ue.UiControlID = uc.UiControlID AND uc.UiControlName = @UiControlName
	WHERE
		c.IsTest = 0
		AND
		@DateStart <= ue.EventTime AND ue.EventTime < @DateEnd
	GROUP BY
		CASE WHEN c.BrokerID IS NULL THEN c.ReferenceSource ELSE 'brk' END,
		c.GoogleCookie,
		c.BrokerID
END
GO
