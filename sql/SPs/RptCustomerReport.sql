IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCustomerReport]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCustomerReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptCustomerReport] 
	(@DateStart DATETIME)
AS
BEGIN
	SELECT
		Name,
		CASE WHEN WizardStep=4 THEN 1 ELSE 0 END AS IsSuccessfullyRegistered,
		AccountNumber,
		Status,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), CreditSum)), 1), 2) CreditSum,
		(CASE WHEN BrokerID IS NULL THEN ReferenceSource ELSE 'Broker' END) AS ReferenceSource
	FROM
		Customer
	WHERE
		CONVERT(DATE, @DateStart) <= GreetingMailSentDate
END
GO
