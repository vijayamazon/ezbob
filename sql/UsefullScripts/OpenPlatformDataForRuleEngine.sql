
INSERT INTO [dbo].[I_InvestorRule]([UserID],[RuleType],[InvestorID],[MemberNameSource],[MemberNameTarget],[LeftParamID],[RightParamID],[Operator],[IsRoot])
     VALUES(1,1,null,null,null,2,3,1,1)
GO

INSERT INTO [dbo].[I_InvestorRule]([UserID],[RuleType],[InvestorID],[MemberNameSource],[MemberNameTarget],[LeftParamID],[RightParamID],[Operator],[IsRoot])
     VALUES (1,1,null,'ManagerApprovedSum','Balance',null,null,3,0)

GO
INSERT INTO [dbo].[I_InvestorRule]([UserID],[RuleType],[InvestorID],[MemberNameSource],[MemberNameTarget],[LeftParamID],[RightParamID],[Operator],[IsRoot])
     VALUES (1,1,null,'ManagerApprovedSum','DailyInvestmentAllowed',null,null,3,0)

GO

INSERT INTO [dbo].[I_Parameter]
           ([Name]
           ,[ValueType]
           ,[DefaultValue]
           ,[MaxLimit]
           ,[MinLimit])
     VALUES
           ('Balance', 'double', 0, null, 0)
GO

INSERT INTO [dbo].[I_Parameter]
           ([Name]
           ,[ValueType]
           ,[DefaultValue]
           ,[MaxLimit]
           ,[MinLimit])
     VALUES
           ('DailyInvestmentAllowed', 'double', 0, null , 0)
GO

INSERT INTO [dbo].[I_InvestorParams]
           ([InvestorID]
           ,[ParameterID]
           ,[Value]
           ,[Type])
     VALUES
			(1, 1, 500, 1)
GO

INSERT INTO [dbo].[I_InvestorParams]
           ([InvestorID]
           ,[ParameterID]
           ,[Value]
           ,[Type])
     VALUES
			(1, 2, 200, 1)
GO




