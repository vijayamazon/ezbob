UPDATE dbo.MP_YodleeGroupRuleMap
SET GroupId = 7
WHERE GroupId = 11 AND 	RuleId = 1 AND Literal = '\w{3}\d{10}[a-zA-Z]\d{3}' AND IsRegex = 1
GO

UPDATE dbo.MP_YodleeGroupRuleMap
SET RuleId = 1
WHERE GroupId = 7 AND RuleId = 5 AND Literal = 'hrmc' AND IsRegex = 0
GO

IF NOT EXISTS (SELECT * FROM MP_YodleeGroupRuleMap WHERE GroupId=7 AND RuleId=5 AND Literal='vat')
	INSERT INTO dbo.MP_YodleeGroupRuleMap (GroupId, RuleId, Literal, IsRegex) VALUES(7, 5, 'vat', 0)
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Priority' and Object_ID = Object_ID(N'MP_YodleeGroup'))
BEGIN 
ALTER TABLE MP_YodleeGroup ADD Priority INT 
END 

GO

UPDATE MP_YodleeGroup SET Priority = 1 WHERE Id IN (1,2) AND Priority IS NULL
UPDATE MP_YodleeGroup SET Priority = 2 WHERE Id IN (3) AND Priority IS NULL
UPDATE MP_YodleeGroup SET Priority = 3 WHERE Id IN (4,5,6) AND Priority IS NULL
UPDATE MP_YodleeGroup SET Priority = 4 WHERE Id IN (7) AND Priority IS NULL
UPDATE MP_YodleeGroup SET Priority = 5 WHERE Id IN (8,9) AND Priority IS NULL
UPDATE MP_YodleeGroup SET Priority = 6 WHERE Id IN (10) AND Priority IS NULL
UPDATE MP_YodleeGroup SET Priority = 7 WHERE Id IN (11) AND Priority IS NULL
