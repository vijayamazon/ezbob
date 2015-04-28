
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AutoDecisionTrail') IS NOT NULL
	DROP VIEW AutoDecisionTrail
GO

CREATE VIEW AutoDecisionTrail AS
	SELECT dtl.TrailID,
		   dtl.CustomerID, 
		   dbo.udfGetTrailNotes(dtl.TrailID) TrailNotes, 
		   dtl.DecisionTime, 
		   d.DecisionName,
		   dtl.InputData,
		   ds.DecisionStatus,
		   dtt.TrailTag
	FROM DecisionTrail dtl
	LEFT JOIN Decisions d ON d.DecisionID = dtl.DecisionID
	LEFT JOIN DecisionStatuses ds ON ds.DecisionStatusID = dtl.DecisionStatusID
	LEFT JOIN DecisionTrailTags dtt ON dtt.TrailTagID = dtl.TrailTagID 
	WHERE dtl.IsPrimary=1
GO
