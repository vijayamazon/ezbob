IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_Attach_DocType]') AND type in (N'U'))
DROP TABLE [dbo].[App_Attach_DocType]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[App_Attach_DocType](
	[AttachmentTypeId] [int] IDENTITY(1,1) NOT NULL,
	[AttachmentType] [nvarchar](512) NOT NULL,
	[AttachmentGroup] [nvarchar](512) NULL,
 CONSTRAINT [PK_Application_Attachment_DocumentType] PRIMARY KEY CLUSTERED 
(
	[AttachmentTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
