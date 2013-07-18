IF (SELECT COUNT(*) FROM ConfigurationVariables cv WHERE cv.Name = 'GreetingMailSendViaMandrill')  = 0 
	INSERT INTO ConfigurationVariables
	(
		Name,
		[Value],
		[Description]
	)
	VALUES
	(
		'GreetingMailSendViaMandrill',
		'Yes',
		'Enable sending greeting mail with Mandrill'
	)