IF OBJECT_ID('GetBasicCustomerOrBrokerData') IS NULL
	EXECUTE('CREATE PROCEDURE GetBasicCustomerOrBrokerData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetBasicCustomerOrBrokerData
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT BrokerID FROM Broker WHERE BrokerID = @CustomerId)
	BEGIN
		SELECT
			b.BrokerID AS Id,
			b.ContactName AS FirstName,
			b.ContactName AS Surname,
			b.ContactName AS Fullname,
			b.ContactEmail AS Mail,
			CONVERT(BIT, 1) AS  IsOffline,
			0 AS NumOfLoans,
			'' AS RefNumber,
			b.ContactMobile AS MobilePhone,
			b.ContactOtherPhone AS DaytimePhone,
			b.IsTest,
			'' AS Postcode,
			'' AS City,
			b.BrokerID AS UserID,
			CONVERT(BIT, 0) AS IsWhiteLabel,
			CONVERT(BIT, 0) AS IsCampaign,
			CONVERT(BIT, 0) AS BrokerID,
			CONVERT(BIT, 0) AS IsFilledByBroker
		FROM
			Broker b
		WHERE
			b.BrokerID = @CustomerID
	END
	ELSE
		EXECUTE GetBasicCustomerData @CustomerId
END
GO
