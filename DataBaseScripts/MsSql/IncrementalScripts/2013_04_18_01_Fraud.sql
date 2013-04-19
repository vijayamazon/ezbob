SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FraudDetection](
	[Id] [int] NOT NULL,
	[CurrentCustomerId] [int] NOT NULL,
	[InternalCustomerId] [int] NULL,
	[ExternalUserId] [int] NULL,
	[CurrentField] [nvarchar](200) NOT NULL,
	[CompareField] [nvarchar](200) NOT NULL,
	[Value] [nvarchar](500) NOT NULL,
 CONSTRAINT [PK_FraudDetection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_Customer] FOREIGN KEY([CurrentCustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO

ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_Customer]
GO

ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_Customer1] FOREIGN KEY([InternalCustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO

ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_Customer1]
GO

ALTER TABLE [dbo].[FraudDetection]  WITH CHECK ADD  CONSTRAINT [FK_FraudDetection_FraudUser] FOREIGN KEY([ExternalUserId])
REFERENCES [dbo].[FraudUser] ([Id])
GO

ALTER TABLE [dbo].[FraudDetection] CHECK CONSTRAINT [FK_FraudDetection_FraudUser]
GO