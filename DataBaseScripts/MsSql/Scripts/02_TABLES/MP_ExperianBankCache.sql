IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_ExperianBankCache]') AND type in (N'U'))
DROP TABLE [dbo].[MP_ExperianBankCache]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ExperianBankCache](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[KeyData] [nvarchar](500) NULL,
	[LastUpdateDate] [datetime] NULL,
	[Data] [nvarchar](max) NULL,
	[ServiceLogId] [bigint] NULL,
 CONSTRAINT [PK_MP_ExperianBankCache] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
