IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='LandRegistryProd')
   INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('LandRegistryProd', 'False', 'true if use land registry production service, false if use stub test service')

GO 