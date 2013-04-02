IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_Error_History]') AND type in (N'U'))
DROP TABLE [dbo].[Application_Error_History]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Application_Error_History](
	[ApplicationId] [bigint] NOT NULL,
	[ErrorMessage] [nvarchar](3000) NULL,
	[ActionDateTime] [datetime] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Application_Error_History] ADD  CONSTRAINT [DF_Application_Error_History_ActionDateTime]  DEFAULT (getdate()) FOR [ActionDateTime]
GO
