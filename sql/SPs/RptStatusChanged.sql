
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptStatusChanged]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptStatusChanged]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptStatusChanged
@DateStart DATE,
@DateEnd DATE
AS
SELECT 
	C.Id,
	C.Fullname,
	H.Username,
	H.TimeStamp,
	S1.Name OldStatus,
	S.Name NewStatus,
	sum(L.Balance) AS Balance
FROM 
	CustomerStatusHistory H,
	Customer C,
	CustomerStatuses S,
	CustomerStatuses S1,
	Loan L
	
WHERE 
	L.CustomerId = C.Id and
	C.Id = H.CustomerId AND 
	S.Id = H.NewStatus AND 
	S1.Id = H.PreviousStatus AND
	H.TimeStamp >= @DateStart AND
	H.TimeStamp < @DateEnd
GROUP BY 	
	C.Id,
	C.Fullname,
	H.Username,
	H.TimeStamp,
	S1.Name,
	S.Name 

GO