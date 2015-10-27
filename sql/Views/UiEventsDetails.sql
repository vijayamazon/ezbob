SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UiEventsDetails') IS NOT NULL
	DROP VIEW UiEventsDetails
GO

CREATE VIEW UiEventsDetails
AS
	SELECT
		e.UserID,
		a.UiActionName,
		c.UiControlName,
		e.EventTime,
		e.EventRefNum,
		e.ControlHtmlID,
		e.EventArguments,
		e.RemoteIP,
		e.BrowserVersionID,
		e.SessionCookie,
		b.BrowserVersion
	FROM
		UiEvents e 
		INNER JOIN BrowserVersions b ON b.BrowserVersionID = e.BrowserVersionID 
		INNER JOIN UiActions a ON a.UiActionID = e.UiActionID
		INNER JOIN UiControls c ON c.UiControlID = e.UiControlID
GO
