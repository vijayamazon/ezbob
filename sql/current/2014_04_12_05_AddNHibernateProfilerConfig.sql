IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='NHibernateEnableProfiler')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('NHibernateEnableProfiler', 'False', 'NHibernateEnableProfiler')
END
GO
