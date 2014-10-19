namespace EzServiceCrontab {
	using System;
	using System.Collections.Generic;

	internal class JobSet {
		#region public

		#region constructor

		public JobSet() {
			m_oJobs = new SortedDictionary<long, Job>();
		} // constructor

		#endregion constructor

		#region property Contains

		public bool Contains(long nJobID) {
			return m_oJobs.ContainsKey(nJobID);
		} // Contains

		#endregion property Contains

		#region indexer

		public Job this[long nJobID] {
			get { return m_oJobs[nJobID]; }
			set { m_oJobs[nJobID] = value; }
		} // indexer

		#endregion indexer

		#region property Count

		public int Count {
			get { return m_oJobs.Count; }
		} // Count

		#endregion property Count

		#region method Iterate

		public void Iterate(Action<long, Job> oAction) {
			if (oAction == null)
				return;

			foreach (KeyValuePair<long, Job> pair in m_oJobs)
				oAction(pair.Key, pair.Value);
		} // Iterate

		#endregion method Iterate

		#region method HasChanged

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

		#endregion method HasChanged

		#endregion public

		#region private

		private readonly SortedDictionary<long, Job> m_oJobs;

		#endregion private
	} // class JobSet
} // namespace
