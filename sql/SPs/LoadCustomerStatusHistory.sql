IF OBJECT_ID('LoadCustomerStatusHistory') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerStatusHistory AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerStatusHistory
@CustomerID INT,
@DateEnd DATETIME
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
	WHERE
		(@CustomerID IS NULL OR h.CustomerId = @CustomerID)
		AND
		(@DateEnd IS NULL OR h.TimeStamp < @DateEnd)
	ORDER BY
		h.CustomerId,
		h.TimeStamp
END
GO
