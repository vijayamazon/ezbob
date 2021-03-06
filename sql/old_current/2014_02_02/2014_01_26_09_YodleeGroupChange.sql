UPDATE dbo.MP_YodleeGroup
SET MainGroup = 'Revenues' 
	, SubGroup = 'Exception'
	, BaseType = 'credit'
WHERE MainGroup='Exception'
GO


IF NOT EXISTS(SELECT * FROM MP_YodleeGroup WHERE MainGroup='Directors withdraws')
BEGIN 
	INSERT INTO dbo.MP_YodleeGroup(MainGroup, SubGroup, BaseType, Priority) VALUES ('Directors withdraws', NULL, NULL, 8)
	INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES	(12, 5, 'week', 0)
	INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES	(12, 5, 'salar', 0)
	INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES	(12, 2, NULL, 0)
END	
GO

IF NOT EXISTS(SELECT * FROM MP_YodleeGroupRuleMap WHERE GroupId=6 AND RuleId=1)
BEGIN
	INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES	(6, 1, 'salar', 0)
	INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES	(6, 1, 'week', 0)
END 	
GO

