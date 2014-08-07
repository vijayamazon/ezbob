IF NOT EXISTS (SELECT 1 FROM CustomerPropertyStatuses WHERE Description = 'Not yet filled')    
BEGIN
	INSERT INTO CustomerPropertyStatuses (Description, IsOwner, GroupId, IsActive, IsOwnerOfMainAddress, IsOwnerOfOtherProperties)
	VALUES ('Not yet filled', 0, 0, 0, 0, 0)

	DECLARE @NotFilledId INT
	SELECT @NotFilledId = Id FROM CustomerPropertyStatuses WHERE Description = 'Not yet filled' 
	UPDATE Customer SET PropertyStatusId = @NotFilledId WHERE PropertyStatusId IS NULL
	ALTER TABLE Customer ADD CONSTRAINT DF_PropertyStatus DEFAULT @@IDENTITY FOR PropertyStatusId
END
GO

