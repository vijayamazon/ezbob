-- TABLES

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudAddress]') AND type in (N'U'))
DROP TABLE [dbo].[FraudAddress]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudAddress](
	[Id] [int] NOT NULL,
	[Postcode] [nvarchar](50) NULL,
	[Line1] [nvarchar](200) NULL,
	[Line2] [nvarchar](200) NULL,
	[Line3] [nvarchar](200) NULL,
	[Town] [nvarchar](200) NULL,
	[County] [nvarchar](200) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudAddress] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudBankAccount]') AND type in (N'U'))
DROP TABLE [dbo].[FraudBankAccount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudBankAccount](
	[Id] [int] NOT NULL,
	[BankAccount] [nvarchar](50) NULL,
	[SortCode] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudBankAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudCompany]') AND type in (N'U'))
DROP TABLE [dbo].[FraudCompany]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudCompany](
	[Id] [int] NOT NULL,
	[CompanyName] [nvarchar](200) NULL,
	[RegistrationNumber] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudCompany] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudEmail]') AND type in (N'U'))
DROP TABLE [dbo].[FraudEmail]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudEmail](
	[Id] [int] NOT NULL,
	[Email] [nvarchar](250) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudEmail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudEmailDomain]') AND type in (N'U'))
DROP TABLE [dbo].[FraudEmailDomain]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudEmailDomain](
	[Id] [int] NOT NULL,
	[EmailDomain] [nvarchar](250) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudEmailDomain] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudPhone]') AND type in (N'U'))
DROP TABLE [dbo].[FraudPhone]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudPhone](
	[Id] [int] NOT NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudPhone] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudShop]') AND type in (N'U'))
DROP TABLE [dbo].[FraudShop]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudShop](
	[Id] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudShop] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- 

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudUser]') AND type in (N'U'))
DROP TABLE [dbo].[FraudUser]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudUser](
	[FirstName] [nvarchar](100) NULL,
	[LastName] [nvarchar](100) NULL,
	[Id] [int] NOT NULL,
 CONSTRAINT [PK_FraudUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- FK KEYS

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudAddress_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudAddress]'))
ALTER TABLE [dbo].[FraudAddress] DROP CONSTRAINT [FK_FraudAddress_FraudUser]
GO
ALTER TABLE [dbo].[FraudAddress]  WITH CHECK ADD  CONSTRAINT [FK_FraudAddress_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudAddress] CHECK CONSTRAINT [FK_FraudAddress_FraudUser]
GO

--

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudBankAccount_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudBankAccount]'))
ALTER TABLE [dbo].[FraudBankAccount] DROP CONSTRAINT [FK_FraudBankAccount_FraudUser]
GO
ALTER TABLE [dbo].[FraudBankAccount]  WITH CHECK ADD  CONSTRAINT [FK_FraudBankAccount_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudBankAccount] CHECK CONSTRAINT [FK_FraudBankAccount_FraudUser]
GO

--

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudCompany_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudCompany]'))
ALTER TABLE [dbo].[FraudCompany] DROP CONSTRAINT [FK_FraudCompany_FraudUser]
GO
ALTER TABLE [dbo].[FraudCompany]  WITH CHECK ADD  CONSTRAINT [FK_FraudCompany_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudCompany] CHECK CONSTRAINT [FK_FraudCompany_FraudUser]
GO

--

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudEmailDomain_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudEmailDomain]'))
ALTER TABLE [dbo].[FraudEmailDomain] DROP CONSTRAINT [FK_FraudEmailDomain_FraudUser]
GO
ALTER TABLE [dbo].[FraudEmailDomain]  WITH CHECK ADD  CONSTRAINT [FK_FraudEmailDomain_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudEmailDomain] CHECK CONSTRAINT [FK_FraudEmailDomain_FraudUser]
GO

--

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudEmail_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudEmail]'))
ALTER TABLE [dbo].[FraudEmail] DROP CONSTRAINT [FK_FraudEmail_FraudUser]
GO
ALTER TABLE [dbo].[FraudEmail]  WITH CHECK ADD  CONSTRAINT [FK_FraudEmail_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudEmail] CHECK CONSTRAINT [FK_FraudEmail_FraudUser]
GO

--

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudPhone_FraudPhone]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudPhone]'))
ALTER TABLE [dbo].[FraudPhone] DROP CONSTRAINT [FK_FraudPhone_FraudPhone]
GO
ALTER TABLE [dbo].[FraudPhone]  WITH CHECK ADD  CONSTRAINT [FK_FraudPhone_FraudPhone] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudPhone] CHECK CONSTRAINT [FK_FraudPhone_FraudPhone]
GO

--

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FraudShop_FraudUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[FraudShop]'))
ALTER TABLE [dbo].[FraudShop] DROP CONSTRAINT [FK_FraudShop_FraudUser]
GO
ALTER TABLE [dbo].[FraudShop]  WITH CHECK ADD  CONSTRAINT [FK_FraudShop_FraudUser] FOREIGN KEY([FraudUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO
ALTER TABLE [dbo].[FraudShop] CHECK CONSTRAINT [FK_FraudShop_FraudUser]
GO