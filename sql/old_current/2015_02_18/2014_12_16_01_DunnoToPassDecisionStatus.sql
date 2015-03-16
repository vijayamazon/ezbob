UPDATE dbo.DecisionStatuses
SET DecisionStatus = 'No decision'
WHERE DecisionStatus = 'Pass'
GO
