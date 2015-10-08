namespace TeamCityModels.ActionTypes
{
    using System;

    public interface IBuildArtifacts
    {
        void DownloadArtifactsByBuildId(string buildId, Action<string> downloadHandler);

        ArtifactWrapper ByBuildConfigId(string buildConfigId);
    }
}