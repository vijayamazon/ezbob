namespace TeamCityModels.DomainEntities
{
    using System.Collections.Generic;

    public class SnapshotDependencies
	{
		public override string ToString()
		{
			return "snapshot-dependencies";
		}

		public List<SnapshotDependency> SnapshotDependency { get; set; }
	}
}