GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExperianConsentAgreement](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Template] [nvarchar](max) NOT NULL,
	[CustomerId] [int] NULL,
	[FilePath] [nvarchar](400) NULL,
 CONSTRAINT [PK_ExperianConsentAgreement] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ExperianConsentAgreement]  WITH CHECK ADD  CONSTRAINT [FK_ExperianConsentAgreement_Customer] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customer] ([Id])
GO

ALTER TABLE [dbo].[ExperianConsentAgreement] CHECK CONSTRAINT [FK_ExperianConsentAgreement_Customer]
GO

