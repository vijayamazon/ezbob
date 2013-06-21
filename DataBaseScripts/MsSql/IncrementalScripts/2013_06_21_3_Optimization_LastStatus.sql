GO
ALTER TABLE dbo.Customer ADD
	LastStatus nvarchar(100) NOT NULL CONSTRAINT DF_Customer_LastStatus DEFAULT 'N/A'
GO

UPDATE Customer
SET Customer.LastStatus=d.LastStatus
from
(
 SELECT cr.[CustomerId], isnull(cr.Action, 'N/a') 'LastStatus'
 FROM [DecisionHistory] cr,
 (
  SELECT MAX(Id) 'Id', [CustomerId]
  FROM [DecisionHistory]
  GROUP BY [CustomerId]
 ) t
 WHERE cr.Id=t.id
) d
WHERE Customer.Id=d.[CustomerId]