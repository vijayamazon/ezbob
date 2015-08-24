namespace TeamCity
{
    using TeamCity.ActionTypes;
    using TeamCity.Connection;

    public class TeamCityClient : IClientConnection, ITeamCityClient
    {
        private readonly TeamCityCaller _caller;
        private IBuilds _builds;
        private IProjects _projects;
        private IBuildConfigs _buildConfigs;
        private IServerInformation _serverInformation;
        private IUsers _users;
        private IAgents _agents;
        private IVcsRoots _vcsRoots;
        private IChanges _changes;
        private IBuildArtifacts _artifacts;

        public TeamCityClient(string hostName, bool useSsl = false)
        {
            this._caller = new TeamCityCaller(hostName, useSsl);
        }

        public void Connect(string userName, string password)
        {
            this._caller.Connect(userName, password, false);
        }

        public void ConnectAsGuest()
        {
            this._caller.Connect(string.Empty, string.Empty, true);
        }

        public bool Authenticate()
        {
            return this._caller.Authenticate("/app/rest");
        }

        public IBuilds Builds
        {
            get { return this._builds ?? (this._builds = new Builds(this._caller)); }
        }

        public IBuildConfigs BuildConfigs
        {
            get { return this._buildConfigs ?? (this._buildConfigs = new BuildConfigs(this._caller)); }
        }

        public IProjects Projects
        {
            get { return this._projects ?? (this._projects = new Projects(this._caller)); }
        }

        public IServerInformation ServerInformation
        {
            get { return this._serverInformation ?? (this._serverInformation = new ServerInformation(this._caller)); }
        }

        public IUsers Users
        {
            get { return this._users ?? (this._users = new Users(this._caller)); }
        }

        public IAgents Agents
        {
            get { return this._agents ?? (this._agents = new Agents(this._caller)); }
        }

        public IVcsRoots VcsRoots
        {
            get { return this._vcsRoots ?? (this._vcsRoots = new VcsRoots(this._caller)); }
        }

        public IChanges Changes
        {
            get { return this._changes ?? (this._changes = new Changes(this._caller)); }
        }

        public IBuildArtifacts Artifacts
        {
            get { return this._artifacts ?? (this._artifacts = new BuildArtifacts(this._caller)); }
        }
    }
}
