IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLeads]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLeads]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptLeads] 
	(@DateStart DATETIME, @DateEnd DATETIME)
AS
BEGIN
	SELECT
	c.Id,
	c.GreetingMailSentDate,
	c.CreditResult,
	c.Name,
	c.Fullname,
	w.WizardStepTypeDescription,
	c.DaytimePhone,
	c.MobilePhone,
	c.OverallTurnOver,
	CASE 
		WHEN c.IsOffline IS NULL THEN 'None'
		WHEN  c.IsOffline = 0 THEN 'Online'
		WHEN c.IsOffline = 1 THEN 'Offline' 
	END AS Segment
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
WHERE
	@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	AND
	c.IsTest = 0
ORDER BY
	w.TheLastOne DESC,
	w.WizardStepTypeDescription DESC,
	c.GreetingMailSentDate,
	c.Fullname
END
GO

