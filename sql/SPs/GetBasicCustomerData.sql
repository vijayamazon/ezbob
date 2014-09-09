IF OBJECT_ID('GetBasicCustomerData') IS NULL
	EXECUTE('CREATE PROCEDURE GetBasicCustomerData AS SELECT 1')
GO

ALTER PROCEDURE GetBasicCustomerData
@CustomerId INT
AS
BEGIN
	DECLARE @NumOfLoans INT

	SELECT 
		@NumOfLoans = COUNT(1)
	FROM 
		Loan l
	WHERE 
		l.CustomerId = @CustomerId

	DECLARE @IsCampaign BIT = 0

	IF EXISTS (
		SELECT 
			cc.CustomerId 
		FROM 
			CampaignMail cm INNER JOIN Campaign c ON cm.Name = c.Name 
		INNER JOIN 
			CampaignClients cc ON cc.CampaignId = c.Id 
		WHERE 
			cm.IsCampaign = 1 
			AND
			cc.CustomerId=@CustomerId
			
		UNION
		SELECT
			c.Id AS CustomerId 
		FROM
			Customer c INNER JOIN CampaignMail cm ON cm.Name = c.ReferenceSource 
		WHERE 
			c.Id = @CustomerId 
			AND
			cm.IsCampaign = 0
	) 
	BEGIN
		SET @IsCampaign = 1
	END

	SELECT
		c.Id,
		c.FirstName,
		c.Surname,
		c.Fullname,
		c.Name AS Mail,
		c.IsOffline,
		@NumOfLoans AS NumOfLoans,
		c.RefNumber,
		c.MobilePhone,
		c.DaytimePhone,
		c.IsTest,
		a.Postcode,
		a.Town AS City,
		c.Id AS UserID,
		CASE
			WHEN c.WhiteLabelId IS NOT NULL THEN CAST(1 AS BIT)
			ELSE CAST(0 AS BIT)
		END AS IsWhiteLabel,
		@IsCampaign AS IsCampaign,
		ISNULL(c.BrokerID, 0) AS BrokerID,
		c.FilledByBroker AS IsFilledByBroker
	FROM
		Customer c
		LEFT JOIN CustomerAddress a
			ON c.Id = a.CustomerId
			AND a.addressType = 1
	WHERE
		c.Id = @CustomerId
END
GO
