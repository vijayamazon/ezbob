IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Medals]') AND type in (N'U'))
DROP TABLE [dbo].[Medals]
GO

/****** Object:  Table [dbo].[Medals]    Script Date: 07/25/2012 11:11:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Medals](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Medal] [nvarchar](50) NOT NULL
) ON [PRIMARY]

INSERT INTO dbo.Medals (Medal) VALUES('Silver')
INSERT INTO dbo.Medals (Medal) VALUES('Gold')
INSERT INTO dbo.Medals (Medal) VALUES('Platinum')
INSERT INTO dbo.Medals (Medal) VALUES('Diamond')

GO