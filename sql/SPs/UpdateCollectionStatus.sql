SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UpdateCollectionStatus') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateCollectionStatus AS SELECT 1')
GO


ALTER PROCEDURE UpdateCollectionStatus
@CustomerID INT,
@CollectionStatus INT,
@Now DATETIME
AS
BEGIN
	DECLARE @CurrentStatusIsAutomatic BIT
	DECLARE @CurrentStatus INT 
	DECLARE @StatusChaged BIT = 0
	
	SELECT @CurrentStatusIsAutomatic = s.IsAutomaticStatus, @CurrentStatus = c.CollectionStatus
	FROM Customer c 
	INNER JOIN CustomerStatuses s ON s.Id = c.CollectionStatus 
	WHERE c.Id=@CustomerID
	
	IF(@CurrentStatusIsAutomatic = 1 AND @CurrentStatus <> @CollectionStatus)
	BEGIN
		UPDATE Customer 
		SET CollectionStatus=@CollectionStatus, CollectionDescription = 'Automatic status change' 
		WHERE Id=@CustomerID
		
		INSERT INTO CustomerStatusHistory (UserName, TimeStamp,CustomerId, PreviousStatus,NewStatus,Description) 
		VALUES ('se', @Now,@CustomerID,@CurrentStatus,@CollectionStatus,'Automatic status change')
		
		SET @StatusChaged = 1
	END
	
	SELECT @StatusChaged AS StatusChaged
END

GO

