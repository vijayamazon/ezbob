IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Control_History]') AND type in (N'U'))
DROP TABLE [dbo].[Control_History]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Control_History](
	[HISROTYID] [bigint] IDENTITY(1,1) NOT NULL,
	[APPLICATIONID] [bigint] NOT NULL,
	[STRATEGYID] [int] NOT NULL,
	[USERID] [int] NOT NULL,
	[NODEID] [int] NOT NULL,
	[CHANGESTIME] [datetime] NOT NULL,
	[CONTROLNAME] [nvarchar](max) NOT NULL,
	[CONTROLVALUE] [nvarchar](max) NULL,
	[CURRENTNODEPOSTFIX] [nvarchar](max) NOT NULL,
	[SECURITYAPPID] [bigint] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Control_History] ADD  CONSTRAINT [DF_CONTROL_HISTORY_CHANGESTIME]  DEFAULT (getdate()) FOR [CHANGESTIME]
GO
