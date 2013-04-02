IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerScoringResult]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerScoringResult]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerScoringResult](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[AC_Parameters] [nvarchar](MAX) NULL,
	[AC_Descriptors] [nvarchar](MAX) NULL,
	[Result_Weights] [nvarchar](MAX) NULL,
	[Result_MAXPossiblePoints] [nvarchar](MAX) NULL,
	[Medal] [nvarchar](20) NULL,
	[ScorePoints] [numeric](8, 3) NULL,
	[ScoreResult] [numeric](8, 3) NULL,	
        [ScoreDate] [datetime] default getdate(),
 CONSTRAINT [PK_CustomerScoringResult] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO