IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayRaitingItem_MP_AnalysisFunctionTimePeriod]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayRaitingItem]'))
ALTER TABLE [dbo].[MP_EbayRaitingItem] DROP CONSTRAINT [FK_MP_EbayRaitingItem_MP_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayRaitingItem_MP_AnalysisFunctionTimePeriod] FOREIGN KEY([TimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem] CHECK CONSTRAINT [FK_MP_EbayRaitingItem_MP_AnalysisFunctionTimePeriod]
GO
