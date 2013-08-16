-- CREATE A NEW TABLE "LoanAgreementTemplate" ---
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanAgreementTemplate]') AND type in (N'U'))
DROP TABLE [dbo].[LoanAgreementTemplate]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LoanAgreementTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Template] [nvarchar](max) NULL,
 CONSTRAINT [PK_LoanAgreementTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

-- COPY UNIQUE DATA TO NEW TABLE ---

INSERT INTO [dbo].[LoanAgreementTemplate]
select [Template] from [dbo].[LoanAgreement] group by [TEMPLATE]


-- ADD A NEW COLUMN TO LoanAgreement ---
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'TemplateId' and Object_ID = Object_ID(N'LoanAgreement'))
BEGIN
ALTER TABLE [dbo].[LoanAgreement] ADD
	[TemplateId] int NULL 
END
GO

-- SET TemplateId with the correct values
UPDATE LA
SET LA.TemplateId = LAT.Id 
FROM [dbo].[LoanAgreement] la
INNER JOIN [dbo].[LoanAgreementTemplate] lat
ON LA.[Template] = LAT.[Template]
GO
