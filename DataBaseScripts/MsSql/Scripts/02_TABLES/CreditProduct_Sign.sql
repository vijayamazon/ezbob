IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_Sign]') AND type in (N'U'))
DROP TABLE [dbo].[CreditProduct_Sign]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CreditProduct_Sign](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreditProductId] [int] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[Data] [ntext] NOT NULL,
	[SignedDocument] [ntext] NOT NULL,
	[UserId] [int] NOT NULL,
 CONSTRAINT [PK_CreditProduct_Sign] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[CreditProduct_Sign] ADD  CONSTRAINT [DF_CreditProduct_Sign_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
