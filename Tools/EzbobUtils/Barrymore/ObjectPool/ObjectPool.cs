namespace Ezbob.Utils.ObjectPool {
	using System;
	using System.Collections.Generic;
	using Logger;

	public class ObjectPool<T> where T: class, IPoolable, new() {
		#region public

		#region constructor

		public ObjectPool(int nMaxSize, ASafeLog oLog = null) {
			m_nIDGenerator = 0;
			Log = new SafeLog(oLog);
			m_sName = string.Format("ObjectPool<{0}>", typeof (T));

			if (nMaxSize < 1) {
				string sMsg = string.Format("Cannot create {0}: pool size must be greater than 0.", this.StringifyStatus());

				Log.Alert("{0}", sMsg);
				throw new ArgumentOutOfRangeException("nMaxSize", sMsg);
			} // if

			m_oPool = new Queue<T>();
			m_oLock = new object();

			m_nMaxSize = nMaxSize;
			m_nCurrentlyOut = 0;

			Log.Debug("New {0} has been created with max size of {1}.", this, MaxSize);
		} // constructor

		#endregion constructor

		#region method SetLog

		public virtual void SetLog(ASafeLog oLog) {} // SetLog

		#endregion method SetLog

		#region property Log

		public virtual SafeLog Log { get; private set; } // Log

		#endregion property Log

		#region property MaxSize

		public int MaxSize {
			get {
				lock (m_oLock)
					return m_nMaxSize;
			} // get
			set {
				lock (m_oLock) {
					m_nMaxSize = Math.Max(value, 1);
				} // lock
			} // set
		} // MaxSize

		private int m_nMaxSize;

		#endregion property MaxSize

		#region property CurrentlyOut

		public virtual int CurrentlyOut {
			get {
				lock (m_oLock)
					return m_nCurrentlyOut;
			} // get
		} // CurrentlyOut

		private int m_nCurrentlyOut;

		#endregion property CurrentlyOut

		#region property CurrentSize

		public virtual int CurrentSize {
			get {
				lock (m_oLock)
					return m_nCurrentlyOut + m_oPool.Count;
			} // get
		} // CurrentSize

		#endregion property CurrentSize

		#region method Give

		public virtual T Give() {
			lock (m_oLock) {
				if (m_nCurrentlyOut >= m_nMaxSize) {
					Log.Debug("The pool {0} cannot give any object as everything is currently out.", this.StringifyStatus());
					return null;
				} // if

				T obj;
				string sMsg;

				if (m_oPool.Count > 0) {
					obj = m_oPool.Dequeue();
					sMsg = "previously used";
				}
				else {
					obj = new T {
						PoolItemID = ++m_nIDGenerator,
					};

					sMsg = "new";
				} // if

				m_nCurrentlyOut++;
				Log.Debug("The pool {0} gives a {1} object with {3}({2}).", this.StringifyStatus(), sMsg, obj.PoolItemID, obj.Name);

				return obj;
			} // lock
		} // Give

		#endregion method Give

		#region method Take

		public virtual bool Take(T obj) {
			if (obj == null) {
				Log.Debug("An object to take is null for {0}, no object taken.", this.StringifyStatus());
				return false;
			} // if

			lock (m_oLock) {
				if (m_oPool.Count >= m_nMaxSize) {
					Log.Debug("The pool {0} is full, object {2}({1}) has not been taken.", this.StringifyStatus(), obj.PoolItemID, obj.Name);
					return false;
				} // if

				if (m_nCurrentlyOut < 1) {
					Log.Debug("Currently out count is 0 for {0}, object {2}({1}) has not been taken.", this.StringifyStatus(), obj.PoolItemID, obj.Name);
					return false;
				} // if

				m_nCurrentlyOut--;
				m_oPool.Enqueue(obj);

				Log.Debug("An object {2}({1}) has been stored to {0}.", this.StringifyStatus(), obj.PoolItemID, obj.Name);
				return true;
			} // lock
		} // Take

		#endregion method Take

		#region method ToString

		public override string ToString() {
			lock (m_oLock)
				return StringifyStatus();
		} // ToString

		#endregion method ToString

		#endregion public

		#region protected

		#region method Forget

		protected virtual void Forget(int nHowMany) {
			if (nHowMany <= 0)
				return;

			lock (m_oLock) {
				m_nCurrentlyOut -= nHowMany;

				if (m_nCurrentlyOut < 0)
					m_nCurrentlyOut = 0;

				Log.Debug(
					"{0} item{1} ha{2} been forgotten. Current status: {3}.",
					nHowMany,
					nHowMany == 1 ? "" : "s",
					nHowMany == 1 ? "ve" : "s",
					this.StringifyStatus()
				);
			} // lock
		} // Forget

		#endregion method Forget

		#endregion protected

		#region private

		private readonly Queue<T> m_oPool;
		private readonly object m_oLock;
		private readonly string m_sName;
		private ulong m_nIDGenerator;

		#region method StringifyStatus

		private string StringifyStatus() {
			return string.Format(
				"{0}(out {1}/in {2}/size {3}/max {4})",
				m_sName,
				m_nCurrentlyOut,
				m_oPool.Count,
				m_oPool.Count + m_nCurrentlyOut,
				m_nMaxSize
			);
		} // StringifyStatus

		#endregion method StringifyStatus

		#endregion private
	} // class ObjectPool
} // namespace Ezbob.Utils.ObjectPool
