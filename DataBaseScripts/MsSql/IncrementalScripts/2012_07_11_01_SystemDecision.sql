UPDATE [dbo].[Customer]
   SET 
      [SystemDecision] = 'Approve'
 WHERE [SystemDecision] = 'Qualified'
GO

UPDATE [dbo].[Customer]
   SET 
      [SystemDecision] = 'Reject'
 WHERE [SystemDecision] = 'NotQualified'
GO