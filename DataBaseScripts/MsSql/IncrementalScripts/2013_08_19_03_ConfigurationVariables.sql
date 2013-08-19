IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'AmountToChargeFrom' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('AmountToChargeFrom', 10, 'if > 0 then disables small amounts charging. value entered is the threshold amount.')
END
