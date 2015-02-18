UPDATE dbo.DecisionStatuses
SET DecisionStatus = 'Pass'
WHERE DecisionStatus = 'Dunno'
GO
