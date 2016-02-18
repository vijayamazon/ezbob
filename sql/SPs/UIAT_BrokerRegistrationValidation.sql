SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UIAT_BrokerRegistrationValidation') IS NULL
	EXECUTE('CREATE PROCEDURE UIAT_BrokerRegistrationValidation AS SELECT 1')
GO

ALTER PROCEDURE UIAT_BrokerRegistrationValidation
@brokerMail NVARCHAR(50)
AS
BEGIN
	SELECT
		b.FirmName
		,b.ContactName
		,b.ContactEmail
		,b.ContactMobile
		,b.EstimatedMonthlyApplicationCount
		,b.EstimatedMonthlyClientAmount
		,co.Name AS OriginName
    FROM
		Broker b
	INNER JOIN CustomerOrigin co ON b.OriginID=co.CustomerOriginID
	WHERE
		b.ContactEmail=@brokerMail
END