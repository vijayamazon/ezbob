IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsRegex' and Object_ID = Object_ID(N'MP_YodleeGroupRuleMap'))
BEGIN 
ALTER TABLE MP_YodleeGroupRuleMap ADD IsRegex BIT DEFAULT(0) NOT NULL
END 
GO
UPDATE MP_YodleeGroupRuleMap SET IsRegex=1 WHERE Literal='\w{3}\d{10}[a-zA-Z]\d{3}'
