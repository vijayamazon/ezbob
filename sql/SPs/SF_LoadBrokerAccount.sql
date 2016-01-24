IF object_ID('SF_LoadBrokerAccount') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE SF_LoadBrokerAccount AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SF_LoadBrokerAccount
	@BrokerID INT
AS 
BEGIN
	SELECT
		  b.BrokerID
		, b.FirmName
		, b.FirmRegNum
		, b.ContactName
		, b.ContactEmail
		, b.ContactMobile
		, b.ContactOtherPhone
		, b.SourceRef
		, b.EstimatedMonthlyClientAmount
		, b.Password
		, b.FirmWebSiteUrl
		, b.EstimatedMonthlyApplicationCount
		, b.IsTest
		, b.ReferredBy
		, b.LicenseNumber
		, b.FCARegistered
		, o.Name AS Origin
	FROM 
		dbo.Broker b 
	INNER JOIN 
		CustomerOrigin o ON o.CustomerOriginID = b.OriginID
	WHERE BrokerID = @BrokerID 
END
GO
