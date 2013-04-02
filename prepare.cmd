rmdir App\clients /s /q
rmdir App\service /s /q
rmdir App\web /s /q
rmdir Tools\EnvironmentConfigValidation /s /q
rmdir Tools\IncrementalUpdate /s /q
rmdir Tools\LogViewer /s /q
rmdir Tools\Publisher    /s /q
rmdir Tools\DownloadScript    /s /q
rmdir Lib\Scorto.Core /s /q

robocopy \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients App\clients /e

robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\AdminServer App\web\AdminServer /e
robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\AuditorServer App\web\AuditorServer /e
robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\AuthenticationServer App\web\AuthenticationServer /e
robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\FormsDesignerServer App\web\FormsDesignerServer /e
robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\MasterServer App\web\MasterServer /e
robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\MavenServer App\web\MavenServer /e

robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\service\ App\service /e

robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\tools\ Tools /e
rmdir Tools\ezmanage /s /q

robocopy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\libs\ Lib\Scorto.Core /e

copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\ApplicationMng.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\BNSCommonLib.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\DatabaseLib.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\DPCore.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\PacketDataFramework.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.ApplicationStateLocal.dll Lib\Scorto.Core


copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.DataSource.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.ExternalFilling.Design.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.ExternalFilling.Model.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.Flow.Signal.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.Maven.Application.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.Maven.Base.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.Maven.Mixed.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.PatronAnalytics.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.RegistryScanner.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.Security.UserManagement.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.Service.Scheduler.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.StrategySchedule.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\Scorto.WebService.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\WorkflowIntf.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\clients\Maven\WorkflowLibrary.dll Lib\Scorto.Core

copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\service\agent\Scorto.Flow.Signal.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Binary\Standard\web\EzBob.Web\bin\Scorto.Web.dll Lib\Scorto.Core
copy  \\ezbob\c$\build\EzBob_Release\Sources\Lib\UnitTestUtils\bin\Debug\UnitTestUtils.dll Lib\Scorto.Core