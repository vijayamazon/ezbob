@echo off

set PAUSE=%1

set SERVICE_DIR=c:\ezbobSrv

call:do net stop ScortoService

for %%d in (
	App\service\agent\
	Integration\NodeEzBobLib\bin\Debug\
	Integration\CustomSchedulers\bin\Debug\
	Integration\EzBob.Signals\bin\Debug\
	Integration\EKM\bin\Debug\
	Integration\ChannelGrabberAPI\bin\Debug\
	Integration\ChannelGrabberConfig\bin\Debug
	Integration\ChannelGrabberFrontend\bin\Debug\
	Integration\PayPoint\bin\Debug\
	Integration\YodleeLib\bin\Debug\
	App\service\Servicestarter\bin\Debug\
	Integration\PaymentServices\bin\Debug\
	App\ScheduledServices\bin\Debug\
) do (
	call:do robocopy %%d %SERVICE_DIR% /e
)

call:do copy Lib\Scorto.Core\DBSQLServer.dll %SERVICE_DIR%

call:do copy Integration\ChannelGrabberConfig\channelgrabber.json "%CommonProgramFiles(x86)%"

call:do net start ScortoService

call:say copy-agent is complete.

goto:eof

:say

echo.
echo ***
echo *** %*
echo ***
echo.

goto:eof

:do

call:say %*

%*

call:say Complete: %*

if "%PAUSE%" == "--pause" pause

goto:eof

