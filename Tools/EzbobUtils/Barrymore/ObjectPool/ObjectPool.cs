namespace Ezbob.Utils.ObjectPool {
	using System;
	using System.Collections.Generic;
	using Logger;

	public class ObjectPool<T> where T: class, IPoolable, new() {
		public ObjectPool(int nMaxSize, ASafeLog oLog = null) {
			this.idGenerator = 0;

			var log = oLog.Safe();
			Log = log;

			this.name = string.Format("ObjectPool<{0}>", typeof (T));

			if (nMaxSize < 1) {
				string sMsg = string.Format("Cannot create {0}: pool size must be greater than 0.", StringifyStatus());

				log.Alert("{0}", sMsg);
				throw new ArgumentOutOfRangeException("nMaxSize", sMsg);
			} // if

			this.pool = new Queue<T>();
			this.locker = new object();

			this.m_nMaxSize = nMaxSize;
			this.m_nCurrentlyOut = 0;

			log.Debug("New {0} has been created with max size of {1}.", this, MaxSize);
		} // constructor

		public virtual void SetLog(ASafeLog oLog) {} // SetLog

		public virtual ASafeLog Log { get; private set; } // Log

		public int MaxSize {
			get {
				lock (this.locker)
					return this.m_nMaxSize;
			} // get
			set {
				lock (this.locker) {
					this.m_nMaxSize = Math.Max(value, 1);
				} // locker
			} // set
		} // MaxSize

		private int m_nMaxSize;

		public virtual int CurrentlyOut {
			get {
				lock (this.locker)
					return this.m_nCurrentlyOut;
			} // get
		} // CurrentlyOut

		private int m_nCurrentlyOut;

		public virtual int CurrentSize {
			get {
				lock (this.locker)
					return this.m_nCurrentlyOut + this.pool.Count;
			} // get
		} // CurrentSize

		public virtual T Give() {
			string sMsg = null;
			Severity severity = Severity.Debug;
			T result = null;

			lock (this.locker) {
				if (this.m_nCurrentlyOut >= this.m_nMaxSize) {
					severity = Severity.Info;
					sMsg = string.Format(
						"The pool {0} cannot give any object as everything is currently out.",
						StringifyStatus()
					);
				} else {
					// string newOrUsed;

					if (this.pool.Count > 0) {
						result = this.pool.Dequeue();
						// newOrUsed = "previously used";
					} else {
						result = new T { PoolItemID = ++this.idGenerator, };
						// newOrUsed = "new";
					} // if

					this.m_nCurrentlyOut++;

					//sMsg = string.Format(
					//	"The pool {0} gives a {1} object with {3}({2}).",
					//	StringifyStatus(),
					//	newOrUsed,
					//	result.PoolItemID,
					//	result.Name
					//);
				} // if
			} // locker

			if (sMsg != null)
				Log.Say(severity, "{0}", sMsg);

			return result;
		} // Give

		public virtual bool Take(T obj) {
			if (obj == null) {
				// Log.Debug("An object to take is null for {0}, no object taken.", StringifyStatus());
				return false;
			} // if

			lock (this.locker) {
				if (this.pool.Count >= this.m_nMaxSize) {
					//Log.Debug(
					//	"The pool {0} is full, object {2}({1}) has not been taken.",
					//	StringifyStatus(),
					//	obj.PoolItemID,
					//	obj.Name
					//);
					return false;
				} // if

				if (this.m_nCurrentlyOut < 1) {
					//Log.Debug(
					//	"Currently out count is 0 for {0}, object {2}({1}) has not been taken.",
					//	StringifyStatus(),
					//	obj.PoolItemID,
					//	obj.Name
					//);
					return false;
				} // if

				this.m_nCurrentlyOut--;
				this.pool.Enqueue(obj);

				// Log.Debug("An object {2}({1}) has been stored to {0}.", StringifyStatus(), obj.PoolItemID, obj.Name);
				return true;
			} // locker
		} // Take

		public override string ToString() {
			lock (this.locker)
				return StringifyStatus();
		} // ToString

		protected virtual void Forget(int nHowMany) {
			if (nHowMany <= 0)
				return;

			lock (this.locker) {
				this.m_nCurrentlyOut -= nHowMany;

				if (this.m_nCurrentlyOut < 0)
					this.m_nCurrentlyOut = 0;

				//Log.Debug(
				//	"{0} item{1} ha{2} been forgotten. Current status: {3}.",
				//	nHowMany,
				//	nHowMany == 1 ? "" : "s",
				//	nHowMany == 1 ? "ve" : "s",
				//	StringifyStatus()
				//);
			} // locker
		} // Forget

		private readonly Queue<T> pool;
		private readonly object locker;
		private readonly string name;
		private ulong idGenerator;

		private string StringifyStatus() {
			return string.Format(
				"{0}(out {1}/in {2}/size {3}/max {4})",
				this.name,
				this.m_nCurrentlyOut,
				this.pool.Count,
				this.pool.Count + this.m_nCurrentlyOut,
				this.m_nMaxSize
			);
		} // StringifyStatus
	} // class ObjectPool
} // namespace Ezbob.Utils.ObjectPool
