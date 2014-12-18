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
		
	SELECT @CurrentStatusIsAutomatic = s.IsAutomaticStatus, @CurrentStatus = c.CollectionStatus
	FROM Customer c 
	INNER JOIN CustomerStatuses s ON s.Id = c.CollectionStatus 
	WHERE c.Id=@CustomerID
	
	IF(@CurrentStatusIsAutomatic = 1)
	BEGIN
		UPDATE Customer 
		SET CollectionStatus=@CollectionStatus, CollectionDescription = 'Automatic status change' 
		WHERE Id=@CustomerID
		
		INSERT INTO CustomerStatusHistory (UserName, TimeStamp,CustomerId, PreviousStatus,NewStatus,Description) 
		VALUES ('se', @Now,@CustomerID,@CurrentStatus,@CollectionStatus,'Automatic status change')
	END
END
GO
