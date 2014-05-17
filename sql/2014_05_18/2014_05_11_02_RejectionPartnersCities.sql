IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='RejectionPartnersCities')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('RejectionPartnersCities', 'London,Birmingham', 'comma separated cities list to send partners rejction mail, use "all" to send this mail to all customers')
END