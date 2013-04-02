IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_Requests]') AND type in (N'U'))
DROP TABLE [dbo].[DataSource_Requests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_Requests](
	[RequestId] [int] IDENTITY(1,1) NOT NULL,
	[ApplicationId] [int] NULL,
	[Request] [nvarchar](max) NULL,
	[CreationDate] [datetime] NULL,
 CONSTRAINT [PK_DataSource_Requests] PRIMARY KEY CLUSTERED 
(
	[RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DataSource_Requests] ADD  CONSTRAINT [DF_DataSource_Requests_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
