IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MailTemplateRelation]') AND type in (N'U'))
DROP TABLE [dbo].[MailTemplateRelation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MailTemplateRelation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InternalTemplateName] [nvarchar](200) NOT NULL,
	[MandrillTemplateId] [int] NOT NULL,
 CONSTRAINT [AK_InternalTemplateName] UNIQUE NONCLUSTERED 
(
	[InternalTemplateName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UNIQUE_InternalTemplateName] UNIQUE NONCLUSTERED 
(
	[InternalTemplateName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
