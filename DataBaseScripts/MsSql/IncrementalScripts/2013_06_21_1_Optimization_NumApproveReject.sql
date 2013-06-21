GO
ALTER TABLE dbo.Customer ADD
	NumApproves int NOT NULL CONSTRAINT DF_Customer_NumApproves DEFAULT 0
GO

ALTER TABLE dbo.Customer ADD
	NumRejects int NOT NULL CONSTRAINT DF_Customer_NumRejects DEFAULT 0
GO

ALTER TABLE dbo.Customer SET (LOCK_ESCALATION = TABLE)
GO

UPDATE Customer
SET Customer.NumApproves=t.[Count]
FROM  
(
 select d.CustomerId, count(*) 'Count'
  from [DecisionHistory] d where d.[Action] = 'Approve'
 GROUP BY d.CustomerId 
) t
where Customer.Id=t.CustomerId
GO

UPDATE Customer
SET Customer.NumRejects=t.[Count]
FROM  
(
 select d.CustomerId, count(*) 'Count'
  from [DecisionHistory] d where d.[Action] = 'Reject'
 GROUP BY d.CustomerId 
) t
where Customer.Id=t.CustomerId
GO