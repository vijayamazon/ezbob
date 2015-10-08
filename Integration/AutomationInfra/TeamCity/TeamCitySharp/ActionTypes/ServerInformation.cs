namespace TeamCityModels.ActionTypes
{
    using System.Collections.Generic;
    using System.Text;
    using TeamCityModels.Connection;
    using TeamCityModels.DomainEntities;

    public class ServerInformation : IServerInformation
    {
        private const string ServerUrlPrefix = "/app/rest/server";
        private readonly ITeamCityCaller _caller;

        public ServerInformation(ITeamCityCaller caller)
        {
            this._caller = caller;
        }

        public Server ServerInfo()
        {
            var server = this._caller.Get<Server>(ServerUrlPrefix);
            return server;
        }

        public List<Plugin> AllPlugins()
        {
            var pluginWrapper = this._caller.Get<PluginWrapper>(ServerUrlPrefix + "/plugins");

            return pluginWrapper.Plugin;
        }

        public string TriggerServerInstanceBackup(BackupOptions backupOptions)
        {
            var backupOptionsUrlPart = this.BuildBackupOptionsUrl(backupOptions);
            var url = string.Concat(ServerUrlPrefix, "/backup?", backupOptionsUrlPart);

            return this._caller.StartBackup(url);
        }

        public string GetBackupStatus()
        {
            var url = string.Concat(ServerUrlPrefix, "/backup");

            return this._caller.GetRaw(url);
        }

        private string BuildBackupOptionsUrl(BackupOptions backupOptions)
        {
            return new StringBuilder()
                .Append("fileName=").Append(backupOptions.Filename)
                .Append("&includeBuildLogs=").Append(backupOptions.IncludeBuildLogs)
                .Append("&includeConfigs=").Append(backupOptions.IncludeConfigurations)
                .Append("&includeDatabase=").Append(backupOptions.IncludeDatabase)
                .Append("&includePersonalChanges=").Append(backupOptions.IncludePersonalChanges)
                .ToString();
        }
    }
}