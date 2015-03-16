UPDATE Customer SET
	GreetingMailSentDate = GETUTCDATE()
WHERE
	GreetingMailSentDate IS NULL
GO

IF 1 = COLUMNPROPERTY(OBJECT_ID('Customer', 'U'), 'GreetingMailSentDate', 'AllowsNull')
BEGIN
	EXECUTE SpDropIndicesByColumn 'Customer', 'GreetingMailSentDate'

	ALTER TABLE Customer ALTER COLUMN GreetingMailSentDate DATETIME NOT NULL

	CREATE INDEX IX_Customer_GreetingMailSentDate_NCSARW ON Customer(GreetingMailSentDate)

	CREATE INDEX IX_Customer_IIG_INW ON Customer(IsTest, IsOffline, GreetingMailSentDate)

	CREATE INDEX IX_Customer_IG ON Customer(IsTest, GreetingMailSentDate)

	CREATE INDEX IX_Customer_IGW ON Customer(IsTest, GreetingMailSentDate, WizardStep)

	CREATE INDEX IX_Customer_GreetingMailSentDate ON Customer(GreetingMailSentDate)

	CREATE INDEX IX_Customer_IG_I ON Customer(IsTest, GreetingMailSentDate)

	CREATE INDEX _dta_index_Customer_7_1125579048__K1_6_41 ON Customer(Id, GreetingMailSentDate, MaritalStatus)
END
GO

IF OBJECT_ID('DF_Customer_GreetingMailSentDate') IS NULL
	ALTER TABLE Customer ADD CONSTRAINT DF_Customer_GreetingMailSentDate DEFAULT(GETUTCDATE()) FOR GreetingMailSentDate
GO
