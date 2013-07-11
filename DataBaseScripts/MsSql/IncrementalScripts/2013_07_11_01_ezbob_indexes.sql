/****** Object:  Index [IX_MP_CustomerMarketPlace]    Script Date: 07/11/2013 12:35:14 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]') 
AND name = N'IX_MP_CustomerMarketPlace')
DROP INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC
)
INCLUDE ( [MarketPlaceId],
[DisplayName],
[EliminationPassed],
[UpdateError]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

/****** Object:  Index [IX_MP_CustomerMarketPlace_CUstId]    Script Date: 07/11/2013 12:37:11 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]') 
AND name = N'IX_MP_CustomerMarketPlace_CUstId')
DROP INDEX [IX_MP_CustomerMarketPlace_CUstId] ON [dbo].[MP_CustomerMarketPlace] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace_CUstId] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC,
	[UpdatingEnd] ASC
)
INCLUDE ( [UpdatingStart],
[MarketPlaceId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO


/****** Object:  Index [IX_LoanSchedule_LoanId_Date]    Script Date: 07/11/2013 12:40:02 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LoanSchedule]') 
AND name = N'IX_LoanSchedule_LoanId_Date')
DROP INDEX [IX_LoanSchedule_LoanId_Date] ON [dbo].[LoanSchedule] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_LoanSchedule_LoanId_Date] ON [dbo].[LoanSchedule] 
(
	[LoanId] ASC,
	[Status] ASC
)
INCLUDE ( [Date],
[LoanRepayment]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO

/****** Object:  Index [IX_LOAN_CustId]    Script Date: 07/11/2013 12:44:46 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Loan]') AND name = N'IX_LOAN_CustId')
DROP INDEX [IX_LOAN_CustId] ON [dbo].[Loan] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_LOAN_CustId]    Script Date: 07/11/2013 12:44:46 ******/
CREATE NONCLUSTERED INDEX [IX_LOAN_CustId] ON [dbo].[Loan] 
(
	[CustomerId] ASC
)
INCLUDE ( [Balance]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

