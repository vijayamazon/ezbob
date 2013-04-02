IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MP_CurrencyRateHistory_MP_Currency]') AND parent_object_id = OBJECT_ID(N'[dbo].[MP_CurrencyRateHistory]'))
ALTER TABLE [dbo].[MP_CurrencyRateHistory] DROP CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency]
GO
ALTER TABLE [dbo].[MP_CurrencyRateHistory]  WITH CHECK ADD  CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[MP_Currency] ([Id])
GO
ALTER TABLE [dbo].[MP_CurrencyRateHistory] CHECK CONSTRAINT [FK_MP_CurrencyRateHistory_MP_Currency]
GO
