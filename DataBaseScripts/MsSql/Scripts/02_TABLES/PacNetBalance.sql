IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PacNetBalance]') AND type in (N'U'))
DROP TABLE [dbo].[PacNetBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacNetBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NULL,
	[Amount] [float] NULL,
	[Fees] [float] NULL,
	[CurrentBalance] [float] NULL,
	[IsCredit] [bit] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PacNetBalance] ADD  DEFAULT ((0)) FOR [IsCredit]
GO
