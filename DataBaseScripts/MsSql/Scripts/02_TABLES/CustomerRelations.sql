IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerRelations]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerRelations]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerRelations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[UserName] [nvarchar](100) NULL,
	[Incoming] [bit] NULL,
	[ActionId] [int] NULL,
	[StatusId] [int] NULL,
	[Comment] [varchar](1000) NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_CustomerRelations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
