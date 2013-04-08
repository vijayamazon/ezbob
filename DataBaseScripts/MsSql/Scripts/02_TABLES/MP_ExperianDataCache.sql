IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_ExperianDataCache]') AND type in (N'U'))
DROP TABLE [dbo].[MP_ExperianDataCache]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_ExperianDataCache](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](500) NULL,
	[Surname] [nvarchar](500) NULL,
	[PostCode] [nvarchar](500) NULL,
	[BirthDate] [datetime] NULL,
	[LastUpdateDate] [datetime] NULL,
	[JsonPacket] [nvarchar](max) NULL,
	[JsonPacketInput] [nvarchar](max) NULL,
	[ExperianError] [nvarchar](max) NULL,
	[ExperianScore] [int] NULL,
	[ExperianResult] [nvarchar](500) NULL,
	[ExperianWarning] [nvarchar](max) NULL,
	[ExperianReject] [nvarchar](max) NULL,
	[CompanyRefNumber] [nvarchar](50) NULL,
	[CustomerId] [bigint] NULL,
	[DirectorId] [bigint] NULL,
 CONSTRAINT [PK_MP_ExperianDataCache] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
