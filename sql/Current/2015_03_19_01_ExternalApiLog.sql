
IF OBJECT_ID('ExternalApiLog') IS NULL
BEGIN

CREATE TABLE [dbo].[ExternalApiLog](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Url] [ntext] NULL,
	[RequestId] [nvarchar](80) NOT NULL,
	[Request] [ntext] NULL,
	[Response] [ntext] NOT NULL,
	[StatusCode] [nvarchar](80) NULL,
	[ErrorCode] [nvarchar](40) NULL,
	[ErrorMessage] [nvarchar](256) NULL,
	[CreateDate] [datetime] NOT NULL,
	[CustomerId] [int] NULL DEFAULT (NULL),
	[Source] [nvarchar](20) NULL,
	[Comments] [ntext] NULL
	)
END
GO
