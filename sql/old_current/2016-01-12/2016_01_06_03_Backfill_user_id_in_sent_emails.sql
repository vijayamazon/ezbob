SET QUOTED_IDENTIFIER ON
GO

UPDATE EzbobMailNodeAttachRelation SET
	UserID = c.Id
FROM 
	EzbobMailNodeAttachRelation r
	LEFT JOIN Customer c ON r.ToField = c.Name
WHERE
	c.Id IS NOT NULL
	AND
	r.UserID IS NULL
GO

UPDATE EzbobMailNodeAttachRelation SET
	UserID = b.BrokerID
FROM 
	EzbobMailNodeAttachRelation r
	LEFT JOIN Broker b ON r.ToField = b.ContactEmail
WHERE
	b.BrokerID IS NOT NULL
	AND
	r.UserID IS NULL
GO
