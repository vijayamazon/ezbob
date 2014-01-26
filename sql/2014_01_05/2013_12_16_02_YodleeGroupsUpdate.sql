IF ((SELECT COUNT(*) FROM MP_YodleeGroup) = 10)
BEGIN 
	INSERT INTO MP_YodleeGroup (MainGroup, SubGroup, BaseType) VALUES ('VAT', NULL, NULL)
	DECLARE @VatGroupId INT = (SELECT Id FROM MP_YodleeGroup WHERE MainGroup='VAT')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (@VatGroupId, 1, 'hmrc vat')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (@VatGroupId, 1, '\w{3}\d{10}[a-zA-Z]\d{3}')
	DELETE FROM MP_YodleeGroupRuleMap WHERE GroupId = (SELECT Id FROM MP_YodleeGroup WHERE SubGroup='Taxes') AND Literal='hmrc' AND RuleId=5
END 	

