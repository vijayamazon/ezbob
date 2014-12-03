IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'AutoDecisionID')
BEGIN
	ALTER TABLE CashRequests ADD AutoDecisionID INT NULL
	
	ALTER TABLE CashRequests ADD CONSTRAINT FK_CashRequests_AutoDecision FOREIGN KEY (AutoDecisionID) REFERENCES Decisions(DecisionID)
END
GO
