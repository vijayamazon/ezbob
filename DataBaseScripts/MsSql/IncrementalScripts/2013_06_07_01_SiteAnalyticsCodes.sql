IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteAnalyticsCodes]') AND type in (N'U'))
	RETURN
ELSE
BEGIN
	CREATE TABLE [dbo].[SiteAnalyticsCodes](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] [nvarchar](300) NOT NULL,
		[Description] [nvarchar](300) NULL,
	 CONSTRAINT [PK_SiteAnalyticsCodes] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
