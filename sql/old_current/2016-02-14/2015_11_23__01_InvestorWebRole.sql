IF NOT EXISTS (SELECT * FROM Security_Role WHERE Name='InvestorWeb')
BEGIN
	INSERT INTO Security_Role(Name, Description) VALUES	('InvestorWeb', 'Investor web contact')
END	
GO
