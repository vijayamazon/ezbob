IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MandrillTemplate]') AND type in (N'U'))
DROP TABLE [dbo].MandrillTemplate
GO

CREATE TABLE MandrillTemplate(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	NAME NVARCHAR(200),
	CONSTRAINT PK_MandrillTemplate PRIMARY KEY NONCLUSTERED ([Id]), 	
)