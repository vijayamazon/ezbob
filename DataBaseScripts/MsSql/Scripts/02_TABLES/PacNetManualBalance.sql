IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PacNetManualBalance]') AND type in (N'U'))
DROP TABLE [dbo].[PacNetManualBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacNetManualBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](100) NULL,
	[Amount] [int] NOT NULL,
	[Date] [datetime] NULL,
	[Enabled] [bit] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PacNetManualBalance] ADD  DEFAULT ((1)) FOR [Enabled]
GO
