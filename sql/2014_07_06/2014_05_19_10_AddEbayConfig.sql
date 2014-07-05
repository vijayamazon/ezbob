DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayServiceType')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayServiceType', 'Production', 'Ebay service type')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayDevId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayDevId', '87cffac6-4c2c-4352-bc88-7cdba02b8085', 'Ebay dev id')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayAppId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayAppId', 'test8f473-e17f-4c46-8ee7-22418d11dd2', 'Ebay app id')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayCertId')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayCertId', 'aea6ac5f-7a80-48ff-820d-355f1979a6e5', 'Ebay cert id')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuName')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayRuName', 'test-test8f473-e17f--xjwup', 'Ebay ru name')
	END
END
ELSE
BEGIN
	IF @Environment = 'QA'
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayServiceType')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayServiceType', 'Production', 'Ebay service type')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayDevId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayDevId', '5708fed8-dc37-41c2-9891-35e19ea1a3f7', 'Ebay dev id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayAppId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayAppId', 'Ezbob342e-6544-4f5f-872b-81572859bff', 'Ebay app id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayCertId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayCertId', '37ba3c68-c0ce-4d51-83c0-e0c754999654', 'Ebay cert id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuName')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayRuName', 'Ezbob-Ezbob342e-6544--arxujwkun', 'Ebay ru name')
		END
	END
	ELSE IF @Environment = 'UAT'
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayServiceType')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayServiceType', 'Production', 'Ebay service type')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayDevId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayDevId', 'b322e279-e402-4435-9f8e-dc3e602a1b72', 'Ebay dev id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayAppId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayAppId', 'Scortoe95-67d5-4848-afe4-99ae20ec497', 'Ebay app id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayCertId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayCertId', '90286b4e-1af1-4844-80f1-a97891583bd8', 'Ebay cert id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuName')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayRuName', 'Scorto-Scortoe95-67d5--lffmtxnt', 'Ebay ru name')
		END
	END
	ELSE
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayServiceType')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayServiceType', 'Production', 'Ebay service type')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayDevId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayDevId', 'e5f86656-1547-4745-abc4-09846cb813f2', 'Ebay dev id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayAppId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayAppId', 'test8c677-69fb-4ff4-a22f-06d1b0e001e', 'Ebay app id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayCertId')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayCertId', '7c840fe4-9beb-4d98-b36e-677e8b5aa1fd', 'Ebay cert id')
		END
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayRuName')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayRuName', 'test-test8c677-69fb--hdwtiapem', 'Ebay ru name')
		END
	END
END
GO
