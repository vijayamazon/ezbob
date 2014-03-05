IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOfflineDailyRegistrations]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOfflineDailyRegistrations]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptOfflineDailyRegistrations] 
	(@DateStart DATE,
@DateEnd DATE)
AS
BEGIN
	SELECT
		c.Id,
		c.Name,
		c.GreetingMailSentDate,
		w.WizardStepTypeDescription AS WizardStep
	FROM
		Customer c
		INNER JOIN WizardStepTypes w ON c.WizardStep = w.WizardStepTypeID
	WHERE
		@DateStart <= c.GreetingMailSentDate AND c.GreetingMailSentDate < @DateEnd 
		AND
		c.IsOffline = 1
		AND
		c.IsTest = 0 
	ORDER BY
		c.WizardStep DESC
END
GO
