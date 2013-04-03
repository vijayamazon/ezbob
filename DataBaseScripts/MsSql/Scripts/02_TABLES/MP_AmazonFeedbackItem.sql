IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_AmazonFeedbackItem]') AND type in (N'U'))
DROP TABLE [dbo].[MP_AmazonFeedbackItem]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_AmazonFeedbackItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AmazonFeedbackId] [int] NOT NULL,
	[TimePeriodId] [int] NOT NULL,
	[Count] [int] NULL,
	[Negative] [int] NULL,
	[Positive] [int] NULL,
	[Neutral] [int] NULL,
 CONSTRAINT [PK_MP_AmazonFeedbackItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
