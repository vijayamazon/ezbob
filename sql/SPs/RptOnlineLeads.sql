IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOnlineLeads]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOnlineLeads]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptOnlineLeads] 
	(@DateStart DATETIME,
@DateEnd DATETIME)
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
	c.OverallTurnOver
FROM
	Customer c
	INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
WHERE
	@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd
	AND
	c.IsTest = 0
	AND
	c.IsOffline = 0
ORDER BY
	w.TheLastOne DESC,
	w.WizardStepTypeDescription DESC,
	c.GreetingMailSentDate,
	c.Fullname
END
GO
