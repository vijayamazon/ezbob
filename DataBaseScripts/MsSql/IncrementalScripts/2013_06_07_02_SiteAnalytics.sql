IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteAnalytics]') AND type in (N'U'))
	RETURN
ELSE
BEGIN
	CREATE TABLE [dbo].[SiteAnalytics](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Date] [datetime] NULL,
		[SiteAnalyticsCode] [int] NULL,
		[SiteAnalyticsValue] [int] NULL,
	 CONSTRAINT [PK_SiteAnalytics] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
