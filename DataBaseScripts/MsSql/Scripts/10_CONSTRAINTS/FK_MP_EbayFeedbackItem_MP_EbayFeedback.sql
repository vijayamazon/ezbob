IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayFeedbackItem_MP_EbayFeedback]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayFeedbackItem]'))
ALTER TABLE [dbo].[MP_EbayFeedbackItem] DROP CONSTRAINT [FK_MP_EbayFeedbackItem_MP_EbayFeedback]
GO
ALTER TABLE [dbo].[MP_EbayFeedbackItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayFeedbackItem_MP_EbayFeedback] FOREIGN KEY([EbayFeedbackId])
REFERENCES [dbo].[MP_EbayFeedback] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayFeedbackItem] CHECK CONSTRAINT [FK_MP_EbayFeedbackItem_MP_EbayFeedback]
GO
