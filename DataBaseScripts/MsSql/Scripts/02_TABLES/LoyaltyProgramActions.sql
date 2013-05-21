IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyProgramActions]') AND type in (N'U'))
DROP TABLE [dbo].[LoyaltyProgramActions]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoyaltyProgramActions](
	[ActionID] [int] NOT NULL,
	[ActionName] [nvarchar](20) NOT NULL,
	[ActionDescription] [nvarchar](256) NOT NULL,
	[Cost] [int] NOT NULL,
	[ActionTypeID] [int] NOT NULL,
 CONSTRAINT [PK_LoyaltyProgramActions] PRIMARY KEY CLUSTERED 
(
	[ActionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UNQ_LoyaltyProgramActionName] UNIQUE NONCLUSTERED 
(
	[ActionName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
