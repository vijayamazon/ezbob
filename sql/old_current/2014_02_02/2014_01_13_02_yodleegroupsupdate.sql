IF NOT EXISTS (SELECT * FROM MP_YodleeGroupRuleMap WHERE GroupId=7 AND RuleId=1 AND Literal='hmrc corp tax')
INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES(7, 1, 'hmrc corp tax', 0)
GO

IF NOT EXISTS (SELECT * FROM MP_YodleeGroupRuleMap WHERE GroupId=8 AND RuleId=1 AND Literal='ezbob')
INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES(8, 1, 'ezbob', 0)
GO

IF NOT EXISTS (SELECT * FROM MP_YodleeGroupRuleMap WHERE GroupId=9 AND RuleId=1 AND Literal='ezbob')
INSERT INTO dbo.MP_YodleeGroupRuleMap(GroupId, RuleId, Literal, IsRegex) VALUES(9, 1, 'ezbob', 0)
GO
