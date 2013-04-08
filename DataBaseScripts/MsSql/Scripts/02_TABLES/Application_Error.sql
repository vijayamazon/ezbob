IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Error]') AND type in (N'U'))
DROP TABLE [dbo].[Application_Error]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_Error](
	[ApplicationId] [bigint] NOT NULL,
	[ErrorMessage] [nvarchar](3000) NULL,
	[ActionDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Application_Error] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Application_Error] ADD  CONSTRAINT [DF_Application_Error_ActionDateTime]  DEFAULT (getdate()) FOR [ActionDateTime]
GO
