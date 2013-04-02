INSERT INTO [dbo].[Security_Role] ([Name] ,[Description]) VALUES ('manager', 'Manager')
GO
INSERT INTO [dbo].[Security_Role] ([Name] ,[Description]) VALUES ('crm', 'CRM')
GO

CREATE TABLE dbo.Security_RolePermissionRel
    (
    RoleId int NOT NULL,
    PermissionId int NOT NULL
    )  ON [PRIMARY]
GO
ALTER TABLE dbo.Security_RolePermissionRel SET (LOCK_ESCALATION = TABLE)
GO


CREATE TABLE dbo.Security_Permission
    (
    Id int NOT NULL,
    Name nvarchar(250) NOT NULL,
    Description nvarchar(MAX) NULL
    )  ON [PRIMARY]
     TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Security_Permission ADD CONSTRAINT
    PK_Permission PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Security_Permission SET (LOCK_ESCALATION = TABLE)
GO

GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (1, N'EmailConfirmationButton', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (2, N'CustomerStatus', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (3, N'TestUser', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (4, N'CRM', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (5, N'NewCreditLineButton', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (6, N'CreditLineFields', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (7, N'ApproveReject', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (8, N'Escalate', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (9, N'RecheckMarketplaces', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (10, N'CheckBankAccount', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (11, N'RecheckPayPal', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (12, N'RunCreditBureauChecks', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (13, N'SendingMessagesToClients', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (14, N'EditLoanDetails', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (15, N'RerunningMarketplaces', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (16, N'RerunningCreditCheck', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (17, N'OpenBug', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (18, N'AddBankAccount', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (19, N'ChangeBankAccount', NULL)
GO
INSERT [dbo].[Security_Permission] ([Id], [Name], [Description]) VALUES (20, N'AddDebitCard', NULL)
GO
