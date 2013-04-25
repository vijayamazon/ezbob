IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptBugs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptBugs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE RptBugs
@DateStart    DATETIME,
@DateEnd      DATETIME
AS
BEGIN
	SELECT ROW_NUMBER() OVER (ORDER BY b.DateOpened DESC) AS RowID, c.Name, b.Type, b.DateOpened, b.TextOpened, s.UserName
	FROM Bug b 
	JOIN Customer c ON b.CustomerId = c.Id
	JOIN Security_User s ON b.UnderwriterOpenedId = s.UserId 
	WHERE b.State = 'Opened' 
	ORDER BY b.DateOpened DESC
END
GO
