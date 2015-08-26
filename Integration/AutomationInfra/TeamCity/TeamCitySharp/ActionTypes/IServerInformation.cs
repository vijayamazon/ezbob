namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using TeamCityModels.DomainEntities;

    public interface IServerInformation
    {
        Server ServerInfo();
        List<Plugin> AllPlugins();
        string TriggerServerInstanceBackup(BackupOptions backupOptions);
        string GetBackupStatus();
    }
}