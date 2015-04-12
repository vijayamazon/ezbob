Updating database structure

1. Make sure you have right environment.json in %APPDATA% directory.

2. Run run.cmd. Observe results. The same results are logged in Dabinuto.log.

QA/UAT/production must specify --skip-env argument.

Dabinuto.exe configuration:

Dabinuto [--help] [--force] [--skip-env] [--base <base path>]

--help:             Show this note and exit.
--force:            Ignore last run time and re-run all the scripts.
--skip-env:         Without this argument this programme works only on DEV environment.
--base <base path>: Use specified path as parent of directories listed in app.config (usually it is c:\ezbob\sql).
                    Default value: this executable location.

SQL scripts are executed according the following section in app.config file:

        <SourceFolders>
                <Folders>
                        <add name="types" />
                        <add name="current" />
                        <add name="Triggers" />
                        <add name="Functions" />
                        <add name="Views" />
                        <add name="SPs" />
                </Folders>
        </SourceFolders>

