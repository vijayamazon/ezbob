
INSERT INTO [dbo].[I_InvestorRule]([UserID],[RuleType],[InvestorID],[FuncName],[MemberNameSource],[MemberNameTarget],[LeftParamID],[RightParamID],[Operator],[IsRoot])
     VALUES(1,0,null,null,null,null,2,3,1,1)
GO

INSERT INTO [dbo].[I_InvestorRule]([UserID],[RuleType],[InvestorID],[FuncName],[MemberNameSource],[MemberNameTarget],[LeftParamID],[RightParamID],[Operator],[IsRoot])
     VALUES (1,0,null,null,'InvestmentAmount','Balance',null,null,3,0)

GO
INSERT INTO [dbo].[I_InvestorRule]([UserID],[RuleType],[InvestorID],[FuncName],[MemberNameSource],[MemberNameTarget],[LeftParamID],[RightParamID],[Operator],[IsRoot])
     VALUES (1,0,null,'RuleBadgetLevel',null,null,null,null,7,0)

GO

INSERT INTO [dbo].[I_Parameter]
           ([Name]
           ,[ValueType]
           ,[DefaultValue]
           ,[MaxLimit]
           ,[MinLimit])
     VALUES
           ('DailyInvestmentAllowed', 'double', 0, null, null)
GO

INSERT INTO [dbo].[I_Parameter]
           ([Name]
           ,[ValueType]
           ,[DefaultValue]
           ,[MaxLimit]
           ,[MinLimit])
     VALUES
           ('WeeklyInvestmentAllowed', 'double', 0, null , null)
GO

INSERT INTO [dbo].[I_InvestorParams]
           ([InvestorID]
           ,[ParameterID]
           ,[Value]
           ,[Type]
		   ,[AllowedForConfig])
     VALUES
			(null, 1, 500, 1,1)
GO

INSERT INTO [dbo].[I_InvestorParams]
           ([InvestorID]
           ,[ParameterID]
           ,[Value]
           ,[Type]
		   ,[AllowedForConfig])
     VALUES
			(null, 2, 200, 1,1)
GO


INSERT INTO [dbo].[I_InvestorParams]
           ([InvestorID]
           ,[ParameterID]
           ,[Value]
           ,[Type]
		   ,[AllowedForConfig])
     VALUES
			(null, 2, 200, 1,1)
GO


