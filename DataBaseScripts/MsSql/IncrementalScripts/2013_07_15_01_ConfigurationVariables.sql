IF (SELECT COUNT(*) FROM ConfigurationVariables cv WHERE cv.Name = 'MandrillEnable')  = 0 
	INSERT INTO ConfigurationVariables
	(
		Name,
		[Value],
		[Description]
	)
	VALUES
	(
		'MandrillEnable',
		'No',
		'Enable sending mail with Mandrill'
	)