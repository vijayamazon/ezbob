IF OBJECT_ID('AlibabaSentData') IS NULL
BEGIN

CREATE TABLE [dbo].[AlibabaSentData](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Request] [ntext] NOT NULL,
	[Response] [ntext] NOT NULL,
	[StatusCode] [nvarchar](80) NULL,
	[Signature] [nvarchar](255) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[AlibabaMemberId] [int] NOT NULL,
	[SentDate] [datetime] NOT NULL,
	[Comments] [ntext] NULL,
	[BizTypeCode] [nvarchar](10) NULL DEFAULT ('0001')
)

END
GO