IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'RejectionLastValidLate')
	INSERT INTO ConfigurationVariables(Name, Value, Description) 
	VALUES ('RejectionLastValidLate', '1', 'Used as threshold in rejection exception logic, the max Id in ExperianAccountStatuses for which late late is not counted')
GO
