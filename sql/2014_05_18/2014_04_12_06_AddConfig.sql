IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CheckStoreUniqueness')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CheckStoreUniqueness', 'True', 'Check store uniqueness')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PostcodeConnectionKey')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PostcodeConnectionKey', 'W_AD4D9278AEF24B4FA8E581A493A2C1', 'Postcode connection key')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CaptchaMode')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CaptchaMode', 'simple', 'Captcha mode')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PasswordPolicyType')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PasswordPolicyType', 'simple', 'Password policy type (6-7 min length)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='LandingPageEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('LandingPageEnabled', 'False', 'Landing page enabled')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='GetCashSliderStep')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('GetCashSliderStep', '100', 'Get cash slider step (interval)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MinLoan')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MinLoan', '1000', 'Min loan amount (approval)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='XMinLoan')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('XMinLoan', '100', 'Min loan amount (single) that is also used for rounding')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MaxLoan')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MaxLoan', '20000', 'Max loan (regular underwriter)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ManagerMaxLoan')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ManagerMaxLoan', '55000', 'Max loan (manager underwriter)')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='UpdateOnReapplyLastDays')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('UpdateOnReapplyLastDays', '14', 'Update on reapply last days')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='DummyPostcodeSearchResult')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('DummyPostcodeSearchResult', '', 'Dummy postcode search result')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='DummyAddressSearchResult')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('DummyAddressSearchResult', '', 'Dummy address search result')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PayPalEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PayPalEnabled', 'True', 'PayPal enabled')
END
GO



DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='WizardTopNaviagtionEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('WizardTopNaviagtionEnabled', 'True', 'Wizard top naviagtion enabled - for testing only')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AskvilleEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AskvilleEnabled', 'False', 'Askville enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TargetsEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TargetsEnabled', 'False', 'Targets enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayPixelEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayPixelEnabled', 'False', 'Ebay pixel enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TaboolaPixelEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TaboolaPixelEnabled', 'False', 'Taboola pixel enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TradeTrackerPixelEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TradeTrackerPixelEnabled', 'False', 'Trade tracker pixel enabled')
	END
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='WizardTopNaviagtionEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('WizardTopNaviagtionEnabled', 'False', 'Wizard top naviagtion enabled - for testing only')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AskvilleEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AskvilleEnabled', 'True', 'Askville enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TargetsEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TargetsEnabled', 'True', 'Targets enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='EbayPixelEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('EbayPixelEnabled', 'True', 'Ebay pixel enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TaboolaPixelEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TaboolaPixelEnabled', 'True', 'Taboola pixel enabled')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TradeTrackerPixelEnabled')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TradeTrackerPixelEnabled', 'True', 'Trade tracker pixel enabled')
	END
END
GO
