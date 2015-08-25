namespace TeamCity.ActionTypes
{
    using System.Collections.Generic;
    using TeamCity.DomainEntities;

    public interface IServerInformation
    {
        Server ServerInfo();
        List<Plugin> AllPlugins();
        string TriggerServerInstanceBackup(BackupOptions backupOptions);
        string GetBackupStatus();
    }
}