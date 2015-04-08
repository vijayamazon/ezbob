SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AutoDecisionTrace') IS NOT NULL
	DROP VIEW AutoDecisionTrace
GO

CREATE VIEW AutoDecisionTrace AS
	SELECT dtc.TraceID,
		   dtl.TrailID,
		   dtc.Position,
		   dtc.Comment, 
		   dtname.TraceName, 
		   ds.DecisionStatus, 
		   dtc.HasLockedDecision
	FROM DecisionTrail dtl
	LEFT JOIN DecisionTrace dtc ON dtl.TrailID = dtc.TrailID
	LEFT JOIN DecisionStatuses ds ON ds.DecisionStatusID = dtc.DecisionStatusID 
	LEFT JOIN DecisionTraceNames dtname ON dtname.TraceNameID = dtc.TraceNameID
	WHERE dtl.IsPrimary=1
GO