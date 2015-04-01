UPDATE ConfigurationVariables SET
	Value = 'https://api.sageone.com/oauth2/token',
	Description = 'Sage OAuth token endpoint. Use fully qualified url, i.e. including schema and host name (e.g. https://api.sageone.com/oauth2/token)'
WHERE
	Name = 'SageOAuthTokenEndpoint'
GO

UPDATE ConfigurationVariables SET
	Value = 'https://www.sageone.com/oauth2/auth',
	Description = 'Sage OAuth authorize endpoint. Use fully qualified url, i.e. including schema and host name (e.g. https://www.sageone.com/oauth2/auth)'
WHERE
	Name = 'SageOAuthAuthorizationEndpoint'
GO
