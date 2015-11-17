IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('CashRequestsHistory') AND name = 'IDX_CashRequestsHistory_Decision')
	DROP INDEX CashRequestsHistory.IDX_CashRequestsHistory_Decision
GO

CREATE INDEX IDX_CashRequestsHistory_Decision ON CashRequestsHistory (Id, UnderwriterDecision, CashRequestHistoryID)
GO
 