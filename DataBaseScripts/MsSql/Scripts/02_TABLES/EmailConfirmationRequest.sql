IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailConfirmationRequest]') AND type in (N'U'))
DROP TABLE [dbo].[EmailConfirmationRequest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailConfirmationRequest](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [int] NULL,
	[Date] [datetime] NOT NULL,
	[State] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_EmailConfirmationRequest] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
