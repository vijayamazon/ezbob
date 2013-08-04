IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FraudAddress]') AND type in (N'U'))
DROP TABLE [dbo].[FraudAddress]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FraudAddress](
	[Id] [int] NOT NULL,
	[Postcode] [nvarchar](50) NULL,
	[Line1] [nvarchar](200) NULL,
	[Line2] [nvarchar](200) NULL,
	[Line3] [nvarchar](200) NULL,
	[Town] [nvarchar](200) NULL,
	[County] [nvarchar](200) NULL,
	[FraudUserId] [int] NOT NULL,
 CONSTRAINT [PK_FraudAddress] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
