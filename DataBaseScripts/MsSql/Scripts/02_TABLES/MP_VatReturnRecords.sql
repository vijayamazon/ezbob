IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_VatReturnRecords]') AND type in (N'U'))
DROP TABLE [dbo].[MP_VatReturnRecords]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MP_VatReturnRecords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerMarketPlaceId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[CustomerMarketPlaceUpdatingHistoryRecordId] [int] NOT NULL,
	[Period] [nvarchar](256) NOT NULL,
	[DateFrom] [datetime] NOT NULL,
	[DateTo] [datetime] NOT NULL,
	[DateDue] [datetime] NOT NULL,
	[BusinessId] [int] NOT NULL,
	[RegistrationNo] [bigint] NOT NULL,
 CONSTRAINT [PK_VatReturnRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MP_VatReturnRecords] ADD  CONSTRAINT [DF_VatReturnRecord_RegNo]  DEFAULT ((0)) FOR [RegistrationNo]
GO
