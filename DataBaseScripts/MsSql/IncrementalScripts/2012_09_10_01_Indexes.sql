IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_Alert]') 
AND name = N'IX_MP_Alert_CustId')
DROP INDEX [IX_MP_Alert_CustId] ON [dbo].[MP_Alert] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_MP_Alert_CustId] ON [dbo].[MP_Alert] 
(
	[CustomerId] ASC
)
INCLUDE ( [StrategyStartedDate]) 
GO


/*-----------------*/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Export_Results]') 
AND name = N'IX_Export_Results_FType')
DROP INDEX [IX_Export_Results_FType] ON [dbo].[Export_Results] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_Export_Results_FType] ON [dbo].[Export_Results] 
(
	[FileType] ASC,
	[ApplicationId] ASC
)
INCLUDE ( [SourceTemplateId])
GO