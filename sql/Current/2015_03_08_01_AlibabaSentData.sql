IF OBJECT_ID('AlibabaSentData') IS NULL
BEGIN

CREATE TABLE [dbo].[AlibabaSentData](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Request] [ntext]  NULL,
	[Response] [ntext]  NULL,
	[StatusCode] [nvarchar](80) NULL,
	[ErrorCode] [nvarchar](40) NULL ,
	[ErrorMessage] [nvarchar](256) NULL ,
	[Signature] [nvarchar](255) NULL ,
	[CustomerId] [int] NOT NULL,
	[AlibabaMemberId] [int] NOT NULL,
	[SentDate] [datetime] NOT NULL,
	[Comments] [ntext] NULL,
	[BizTypeCode] [nvarchar](10) NULL 
)

END
GO