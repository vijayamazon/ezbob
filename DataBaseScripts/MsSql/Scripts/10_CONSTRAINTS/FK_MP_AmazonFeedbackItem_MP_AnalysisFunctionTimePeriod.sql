IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_AmazonFeedbackItem_MP_AnalysisFunctionTimePeriod]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_AmazonFeedbackItem]'))
ALTER TABLE [dbo].[MP_AmazonFeedbackItem] DROP CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AnalysisFunctionTimePeriod]
GO
ALTER TABLE [dbo].[MP_AmazonFeedbackItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AnalysisFunctionTimePeriod] FOREIGN KEY([TimePeriodId])
REFERENCES [dbo].[MP_AnalysisFunctionTimePeriod] ([Id])
GO
ALTER TABLE [dbo].[MP_AmazonFeedbackItem] CHECK CONSTRAINT [FK_MP_AmazonFeedbackItem_MP_AnalysisFunctionTimePeriod]
GO
