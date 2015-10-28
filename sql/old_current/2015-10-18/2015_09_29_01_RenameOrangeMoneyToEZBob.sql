UPDATE  EsignTemplates SET Template = convert(
    varbinary(MAX),
    REPLACE(
        convert(varchar(MAX), Template),
         'Orange Money Ltd',
         'EZBob Ltd'
    )
)