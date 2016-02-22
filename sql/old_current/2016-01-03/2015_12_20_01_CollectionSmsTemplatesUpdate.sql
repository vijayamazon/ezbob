IF NOT EXISTS (SELECT * FROM CollectionSmsTemplate WHERE IsActive=1 AND Template='{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693773')
BEGIN
	UPDATE CollectionSmsTemplate SET IsActive=0 WHERE IsActive=1
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay0'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay0'
		, 1
		, 2
		, '{0}. Please call Everline to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay7'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay7'
		, 1
		, 2
		, '{0}. Please call Everline to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay21'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693771'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay21'
		, 1
		, 2
		, '{0}.Please call Everline to discuss the status of your account as soon as possible on 02037693771'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay31'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693771'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay31'
		, 1
		, 2
		, '{0}.Please call Everline to discuss the status of your account as soon as possible on 02037693771'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay3'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay3'
		, 1
		, 2
		, '{0}. Please call Everline to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay10'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay10'
		, 1
		, 2
		, '{0}. Please call Everline to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay13'
		, 1
		, 1
		, '{0}.Please call ezbob to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
	
	
	INSERT INTO dbo.CollectionSmsTemplate
		(
		Type
		, IsActive
		, OriginID
		, Template
		, Comment
		)
	VALUES
		(
		'CollectionDay13'
		, 1
		, 2
		, '{0}. Please call Everline to discuss the status of your account as soon as possible on 02037693773'
		, '{0} - FirstName'
		)
END
GO
