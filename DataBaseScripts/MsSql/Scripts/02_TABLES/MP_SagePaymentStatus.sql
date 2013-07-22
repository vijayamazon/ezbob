IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_SagePaymentStatus]') AND type in (N'U'))
DROP TABLE [dbo].[MP_SagePaymentStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_SagePaymentStatus](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SageId] [int] NOT NULL,
	[name] [nvarchar](250) NULL,
 CONSTRAINT [PK_MP_SagePaymentStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
