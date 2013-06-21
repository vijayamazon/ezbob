GO
ALTER TABLE dbo.Customer ADD
	SystemCalculatedSum decimal(18, 4) NOT NULL CONSTRAINT DF_Customer_SystemCalculatedSum DEFAULT 0
GO

ALTER TABLE dbo.Customer ADD
	ManagerApprovedSum decimal(18, 4) NOT NULL CONSTRAINT DF_Customer_ManagerApprovedSum DEFAULT 0
GO

ALTER TABLE dbo.Customer SET (LOCK_ESCALATION = TABLE)
GO

UPDATE Customer
SET Customer.SystemCalculatedSum=d.SystemCalculatedSum
from
(
 SELECT cr.IdCustomer, isnull(cr.SystemCalculatedSum, 0) 'SystemCalculatedSum'
 FROM [CashRequests] cr,
 (
  SELECT MAX(Id) 'Id', IdCustomer
  FROM [CashRequests]
  GROUP BY IdCustomer
 ) t
 WHERE cr.Id=t.id
) d
WHERE Customer.Id=d.IdCustomer
GO

UPDATE Customer
SET Customer.ManagerApprovedSum=d.ManagerApprovedSum
from
(
 SELECT cr.IdCustomer, isnull(cr.ManagerApprovedSum, 0) 'ManagerApprovedSum'
 FROM [CashRequests] cr,
 (
  SELECT MAX(Id) 'Id', IdCustomer
  FROM [CashRequests]
  GROUP BY IdCustomer
 ) t
 WHERE cr.Id=t.id
) d
WHERE Customer.Id=d.IdCustomer
GO