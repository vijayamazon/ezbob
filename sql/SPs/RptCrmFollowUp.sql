IF OBJECT_ID('RptCrmFollowUp') IS NULL
	EXECUTE('CREATE PROCEDURE RptCrmFollowUp AS SELECT 1')
GO

ALTER PROCEDURE RptCrmFollowUp
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN

SELECT c.Id CustomerId, c.Name Email, f.FollowUpDate, f.Comment
FROM Customer c JOIN CustomerRelationState s ON c.Id = s.CustomerId 
			    JOIN CustomerRelationFollowUp f ON f.Id = s.LastFollowUpId
WHERE
	c.IsTest = 0
AND
	s.IsFollowUp = 1
AND
	f.FollowUpDate <= @DateEnd
ORDER BY
	f.FollowUpDate	  
	
END 	

GO