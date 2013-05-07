delete from ConfigurationVariables where name = 'EnableAutomaticRejection'
GO
INSERT INTO ConfigurationVariables (Name, [Value], [Description])
VALUES ('EnableAutomaticRejection', '0', 'if "1" system will reject customers automatically without any Underwriter actions')