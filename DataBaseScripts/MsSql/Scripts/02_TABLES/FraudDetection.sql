IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudDetection]') AND type in (N'U'))
DROP TABLE [dbo].[FraudDetection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudDetection](
	[Id] [int] NOT NULL,
	[CurrentCustomerId] [int] NOT NULL,
	[InternalCustomerId] [int] NULL,
	[ExternalUserId] [int] NULL,
	[CurrentField] [nvarchar](200) NOT NULL,
	[CompareField] [nvarchar](200) NOT NULL,
	[Value] [nvarchar](500) NOT NULL,
	[DateOfCheck] [datetime] NULL,
	[Concurrence] [nvarchar](250) NULL,
 CONSTRAINT [PK_FraudDetection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
