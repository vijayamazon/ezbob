﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudShop]') AND type in (N'U'))
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
