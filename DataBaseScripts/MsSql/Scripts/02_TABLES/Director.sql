IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Director]') AND type in (N'U'))
DROP TABLE [dbo].[Director]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Director](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[Name] [nvarchar](512) NOT NULL,
	[DateOfBirth] [datetime] NULL,
	[Middle] [nvarchar](512) NULL,
	[Surname] [nvarchar](512) NULL,
	[Gender] [char](1) NULL
) ON [PRIMARY]
GO
