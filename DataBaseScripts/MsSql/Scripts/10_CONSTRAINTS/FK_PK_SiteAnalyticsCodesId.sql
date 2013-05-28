IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PK_SiteAnalyticsCodesId]') AND parent_object_id = OBJECT_ID(N'[dbo].[SiteAnalytics]'))
ALTER TABLE [dbo].[SiteAnalytics] DROP CONSTRAINT [FK_PK_SiteAnalyticsCodesId]
GO
ALTER TABLE [dbo].[SiteAnalytics]  WITH CHECK ADD  CONSTRAINT [FK_PK_SiteAnalyticsCodesId] FOREIGN KEY([SiteAnalyticsCode])
REFERENCES [dbo].[SiteAnalyticsCodes] ([Id])
GO
ALTER TABLE [dbo].[SiteAnalytics] CHECK CONSTRAINT [FK_PK_SiteAnalyticsCodesId]
GO
