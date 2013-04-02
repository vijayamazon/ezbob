IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_EbayRaitingItem_MP_EbayFeedback]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_EbayRaitingItem]'))
ALTER TABLE [dbo].[MP_EbayRaitingItem] DROP CONSTRAINT [FK_MP_EbayRaitingItem_MP_EbayFeedback]
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem]  WITH CHECK ADD  CONSTRAINT [FK_MP_EbayRaitingItem_MP_EbayFeedback] FOREIGN KEY([EbayFeedbackId])
REFERENCES [dbo].[MP_EbayFeedback] ([Id])
GO
ALTER TABLE [dbo].[MP_EbayRaitingItem] CHECK CONSTRAINT [FK_MP_EbayRaitingItem_MP_EbayFeedback]
GO
