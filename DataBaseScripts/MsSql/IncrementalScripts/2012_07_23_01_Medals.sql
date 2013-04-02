IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Medals]') AND type in (N'U'))
DROP TABLE [dbo].[Medals]
GO

CREATE TABLE [dbo].[Medals](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Medal] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO