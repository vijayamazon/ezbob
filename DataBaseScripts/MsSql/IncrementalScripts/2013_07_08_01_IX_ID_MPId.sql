/****** Object:  Index [IX_ID_MPId]    Script Date: 07/08/2013 11:20:37 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_PayPalTransaction]') AND name = N'IX_ID_MPId')
DROP INDEX [IX_ID_MPId] ON [dbo].[MP_PayPalTransaction] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_ID_MPId]    Script Date: 07/08/2013 11:20:37 ******/
CREATE NONCLUSTERED INDEX [IX_ID_MPId] ON [dbo].[MP_PayPalTransaction] 
(
 [Id] ASC,
 [CustomerMarketPlaceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO