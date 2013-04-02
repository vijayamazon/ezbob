IF ((SELECT COUNT (*) FROM ConfigurationVariables
    WHERE NAME='BWABusinessCheck')=0)
    INSERT INTO ConfigurationVariables (NAME, [Value],[Description])
    VALUES ('BWABusinessCheck', 1, 'if "1" BWA Experian check will be performed')