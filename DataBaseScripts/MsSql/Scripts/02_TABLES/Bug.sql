IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Bug]') AND type in (N'U'))
DROP TABLE [dbo].[Bug]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bug](
	[Id] [int] NOT NULL,
	[CustomerId] [int] NULL,
	[Type] [nvarchar](200) NULL,
	[State] [nvarchar](200) NULL,
	[MarketPlaceId] [int] NULL,
	[DateOpened] [datetime] NULL,
	[DateClosed] [datetime] NULL,
	[TextOpened] [nvarchar](2000) NULL,
	[TextClosed] [nvarchar](2000) NULL,
	[UnderwriterOpenedId] [int] NULL,
	[UnderwriterClosedId] [int] NULL,
	[CreditBureauDirectorId] [int] NULL,
 CONSTRAINT [PK_Bug] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
