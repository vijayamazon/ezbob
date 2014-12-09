namespace EzServiceCrontab {
	using System;
	using System.Collections.Generic;

	internal class JobSet {

		public JobSet() {
			m_oJobs = new SortedDictionary<long, Job>();
		} // constructor

		public bool Contains(long nJobID) {
			return m_oJobs.ContainsKey(nJobID);
		} // Contains

		public Job this[long nJobID] {
			get { return m_oJobs[nJobID]; }
			set { m_oJobs[nJobID] = value; }
		} // indexer

		public int Count {
			get { return m_oJobs.Count; }
		} // Count

		public void Iterate(Action<long, Job> oAction) {
			if (oAction == null)
				return;

			foreach (KeyValuePair<long, Job> pair in m_oJobs)
				oAction(pair.Key, pair.Value);
		} // Iterate

		public bool HasChanged(JobSet oPrevious) {
			if (oPrevious == null)
				return true;

			if (this.Count != oPrevious.Count)
				return true;

			var oIDs = new SortedSet<long>();

			this.Iterate((nJobID, oJob) => oIDs.Add(nJobID));
			oPrevious.Iterate((nJobID, oJob) => oIDs.Add(nJobID));

			foreach (long nJobID in oIDs) {
				if (!this.Contains(nJobID))
					return true;

				if (!oPrevious.Contains(nJobID))
					return true;

				if (this[nJobID].Differs(oPrevious[nJobID]))
					return true;
			} // for each job id

			return false;
		} // HasChanged

		private readonly SortedDictionary<long, Job> m_oJobs;

	} // class JobSet
} // namespace
