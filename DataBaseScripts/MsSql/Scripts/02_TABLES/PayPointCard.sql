IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PayPointCard]') AND type in (N'U'))
DROP TABLE [dbo].[PayPointCard]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayPointCard](
	[Id] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
	[DateAdded] [datetime] NOT NULL,
	[TransactionId] [nvarchar](250) NULL,
	[CardNo] [nvarchar](50) NULL,
	[ExpireDate] [datetime] NULL,
	[ExpireDateString] [nvarchar](50) NULL
) ON [PRIMARY]
GO
