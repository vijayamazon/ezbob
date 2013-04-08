IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_KeyData]') AND type in (N'U'))
DROP TABLE [dbo].[DataSource_KeyData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataSource_KeyData](
	[KeyValueId] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [int] NULL,
	[KeyNameId] [int] NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_DataSource_KeyData] PRIMARY KEY CLUSTERED 
(
	[KeyValueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
