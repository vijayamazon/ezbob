IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EzbobMailNodeAttachRelation]') AND type in (N'U'))
DROP TABLE [dbo].[EzbobMailNodeAttachRelation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EzbobMailNodeAttachRelation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ExportId] [int] NULL,
	[ToField] [nvarchar](200) NULL,
 CONSTRAINT [PK_EzbobMailNodeAttachRelation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
