UPDATE [dbo].[Customer]
   SET 
      [Status] = 'Approved'
 WHERE [Status] = 'Qualified'
GO

UPDATE [dbo].[Customer]
   SET 
      [Status] = 'Rejected'
 WHERE [Status] = 'NotQualified'
GO