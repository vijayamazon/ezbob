IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyProgramActionTypes]') AND type in (N'U'))
DROP TABLE [dbo].[LoyaltyProgramActionTypes]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoyaltyProgramActionTypes](
	[ActionTypeID] [int] NOT NULL,
	[ActionTypeName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_LoyaltyProgramActionTypes] PRIMARY KEY CLUSTERED 
(
	[ActionTypeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UNQ_LoyaltyProgramActionTypes] UNIQUE NONCLUSTERED 
(
	[ActionTypeName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
