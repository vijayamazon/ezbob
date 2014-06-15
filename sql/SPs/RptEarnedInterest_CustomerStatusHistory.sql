IF OBJECT_ID('RptEarnedInterest_CustomerStatusHistory') IS NULL
	EXECUTE('CREATE PROCEDURE RptEarnedInterest_CustomerStatusHistory AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptEarnedInterest_CustomerStatusHistory
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		h.CustomerId AS CustomerID,
		h.TimeStamp AS ChangeDate,
		p.Name AS OldStatus,
		n.Name AS NewStatus
	FROM
		CustomerStatusHistory h
		INNER JOIN CustomerStatuses p ON h.PreviousStatus = p.Id
		INNER JOIN CustomerStatuses n ON h.NewStatus = n.Id
	ORDER BY
		h.CustomerId,
		h.TimeStamp
END
GO
