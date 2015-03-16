IF (EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_YodleeGroup]') AND type in (N'U')) AND (SELECT count(*) FROM MP_YodleeGroup) = 9) 
BEGIN
	ALTER TABLE MP_YodleeOrderItemBankTransaction DROP CONSTRAINT FK_MP_YodleeGroup_MP_YodleeOrderItemBankTransaction
    ALTER TABLE MP_YodleeOrderItemBankTransaction DROP COLUMN EzbobCategory
	DROP TABLE MP_YodleeGroupRuleMap
	DROP TABLE MP_YodleeRule
	DROP TABLE MP_YodleeGroup
END 

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_YodleeGroup]') AND type in (N'U'))
BEGIN
    --DROP TABLE MP_YodleeGroupRuleMap
    --DROP TABLE mp_yodleerule
	--DROP TABLE MP_YodleeGroup
	CREATE TABLE [dbo].[MP_YodleeGroup]
	(
		Id        INT IDENTITY(1,1) NOT NULL,
		MainGroup NVARCHAR(100) NOT NULL,
		SubGroup  NVARCHAR(100),
		BaseType  NVARCHAR(100),
	    CONSTRAINT PK_MP_YodleeGroup PRIMARY KEY (Id)
	) 
	
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Revenues', 'Online', 'credit')
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Revenues', 'Transfers','credit')
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Opex', NULL, 'debit')
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Salaries and Tax', 'Salaries', NULL)
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Salaries and Tax', 'Taxes', NULL)
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Salaries and Tax', 'Directors withdrawal', NULL)
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Corporate tax', NULL, NULL)
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Loan Repayments', 'Receipt', 'credit')
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Loan Repayments', 'Repayment', 'debit')
	INSERT INTO MP_YodleeGroup (MainGroup , SubGroup, BaseType) VALUES ('Exception', NULL, NULL)
	
	CREATE TABLE [dbo].[MP_YodleeRule]
	(
		Id                INT IDENTITY(1,1) NOT NULL,
		RuleDescription   NVARCHAR(100) NOT NULL,
	    CONSTRAINT PK_MP_YodleeRule PRIMARY KEY (Id)
	) 
	
	INSERT INTO MP_YodleeRule (RuleDescription) VALUES ('Include a literal word')
	INSERT INTO MP_YodleeRule (RuleDescription) VALUES ('Include a director last name')
	INSERT INTO MP_YodleeRule (RuleDescription) VALUES ('Transaction is a round figure')
	INSERT INTO MP_YodleeRule (RuleDescription) VALUES ('Not categorized')
	INSERT INTO MP_YodleeRule (RuleDescription) VALUES ('Don''t include a literal word')
	INSERT INTO MP_YodleeRule (RuleDescription) VALUES ('Don''t include a director last name')

	CREATE TABLE [dbo].[MP_YodleeGroupRuleMap]
	(
		Id       INT IDENTITY(1,1) NOT NULL,
		GroupId  INT NOT NULL,
		RuleId   INT NOT NULL,
		Literal  NVARCHAR(100),
	    CONSTRAINT PK_MP_YodleeGroupRuleMap PRIMARY KEY (Id),
	    CONSTRAINT FK_MP_YodleeGroupRuleMap_MP_YodleeGroup FOREIGN KEY (GroupId) REFERENCES MP_YodleeGroup (Id),
	    CONSTRAINT FK_MP_YodleeGroupRuleMap_MP_YodleeRule FOREIGN KEY (RuleId) REFERENCES MP_YodleeRule (Id)
	) 
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (1,1,'paypal')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (1,1,'ebay')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (1,1,'amazon')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (1,1,'playtrade')
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (2,1,'transfer')
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (4,1,'salary')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (4,1,'week')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (4,6, NULL)
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (5,1,'paye')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (5,1,'hmrc')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (5,5,'vat')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (5,5,'corporate')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (5,5,'corp')
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (6,2,NULL)
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (7,1,'corp')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (7,1,'corporate')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (7,5,'hrmc')
	
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'loan')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'funding circle')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'everline')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'wonga')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'iwoca')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'sme invoice')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (8,1,'financ')

	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'loan')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'funding circle')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'everline')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'wonga')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'iwoca')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'sme invoice')
	INSERT INTO MP_YodleeGroupRuleMap (GroupId, RuleId, Literal) VALUES (9,1,'financ')


	--ALTER TABLE MP_YodleeOrderItemBankTransaction DROP CONSTRAINT FK_MP_YodleeGroup_MP_YodleeOrderItemBankTransaction
    --ALTER TABLE MP_YodleeOrderItemBankTransaction DROP COLUMN EzbobCategory
	ALTER TABLE MP_YodleeOrderItemBankTransaction ADD EzbobCategory INT 
	ALTER TABLE MP_YodleeOrderItemBankTransaction ADD CONSTRAINT FK_MP_YodleeGroup_MP_YodleeOrderItemBankTransaction FOREIGN KEY (EzbobCategory) REFERENCES MP_YodleeGroup(Id)
		
END
GO



