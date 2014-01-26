
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsPaymentAccount' and Object_ID = Object_ID(N'MP_MarketplaceType'))
BEGIN 

	ALTER TABLE MP_MarketplaceType 
	ADD IsPaymentAccount BIT NOT NULL DEFAULT(0)
END
GO
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'A7120CB7-4C93-459B-9901-0E95E7281B59' --ebay
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'A4920125-411F-4BB9-A52D-27E8A00D0A3B' --amazon
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = '3FA5E327-FCFD-483B-BA5A-DC1815747A28' --paypal
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = '57ABA690-EDBA-4D95-89CF-13A34B40E2F1' --ekm
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = 'FC8F2710-AEDA-481D-86FF-539DD1FB76E0' --paypoint
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'AFCA0E18-05E3-400F-8AF4-B1BCAE09375C' --volusion
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6' --play
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF' --yodlee
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = '737691E8-5C77-48EF-B01B-7348E24094B6' --freeagent
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'A386F349-8E41-4BA9-B709-90332466D42D' -- shopify
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C' --xero
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = '4966BB57-0146-4E3D-AA24-F092D90B7923' --sage
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = 'A755B4F6-D4EC-4D80-96A2-B2849BD800AC' --kashflow
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 1
	WHERE InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA' --hmrc
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'A660B9CC-8BB1-4A37-9597-507622AEBF9E' --magento
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'AE0BC89A-9884-4025-9D96-2755A6CD10EE' --prestashop
	
	
	UPDATE dbo.MP_MarketplaceType
	SET IsPaymentAccount = 0
	WHERE InternalId = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996' --bigcommerce	
	
GO 

SELECT * FROM MP_MarketplaceType
