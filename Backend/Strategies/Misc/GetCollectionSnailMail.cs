namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using System.IO;
	using Ezbob.Backend.ModelsWithDB;

	public class GetCollectionSnailMail : AStrategy {
		private readonly int collectionSnailMailID;

		public GetCollectionSnailMail(int collectionSnailMailID) {
			this.collectionSnailMailID = collectionSnailMailID;
		}

		public override string Name {
			get { return "Collection Snail Mail File Retrieve"; }
		}

		public CollectionSnailMailMetadataModel Result { get; private set; }

		public override void Execute() {
			try {
				Result = DB.FillFirst<CollectionSnailMailMetadataModel>("GetCollectionSnailMailFileMetadata", 
					CommandSpecies.StoredProcedure,
					new QueryParameter("CollectionSnailMailMetadataID", this.collectionSnailMailID));

				if (Result != null && !string.IsNullOrEmpty(Result.Path)) {
					Result.Content = File.ReadAllBytes(Result.Path);
				}
			}
			catch (Exception e) {
				Log.Error("Error retrieving file for collection snail mail, file id: {0} \n {1}", this.collectionSnailMailID, e);
			} // try
		}//Execute
	}//class
}//ns
