IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Setting]') AND type in (N'U'))
DROP TABLE [dbo].[Application_Setting]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_Setting](
	[Param1DetailNameId] [int] NULL,
	[Param2DetailNameId] [int] NULL,
	[Param3DetailNameId] [int] NULL,
	[Param4DetailNameId] [int] NULL,
	[Param5DetailNameId] [int] NULL,
	[Param6DetailNameId] [int] NULL,
	[Param7DetailNameId] [int] NULL,
	[Param8DetailNameId] [int] NULL,
	[Param9DetailNameId] [int] NULL
) ON [PRIMARY]
GO
