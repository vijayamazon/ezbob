namespace EzBobCommon.NSB {
    using System;
    using System.Threading;

    /// <summary>
    /// Blocks method that should return value supplied by another thread.<br/>
    /// Assumes that another thread will call unlock with required value
    /// </summary>
    /// <typeparam name="T">The class that blocked method returns</typeparam>
    public class Locker<T>
        where T : class {
        private readonly TimeSpan timeout = TimeSpan.FromMinutes(5);
        private readonly CountdownEvent countdown = new CountdownEvent(1);
        private T val;

        /// <summary>
        /// Initializes a new instance of the <see cref="Locker{T}"/> class.
        /// </summary>
        public Locker() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Locker{T}"/> class.
        /// </summary>
        /// <param name="timeOut">The time out - after which the method automatically unlocked</param>
        public Locker(TimeSpan timeOut) {
            this.timeout = timeOut;
        }

        /// <summary>
        /// Blocks calling method until unlocked by another thread.
        /// </summary>
        /// <returns></returns>
        public T Lock() {
            this.countdown.Wait(this.timeout);
            return this.val;
        }

        /// <summary>
        /// Unlocks blocked method with required value
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Unlock(T obj) {
            this.val = obj;
            this.countdown.Signal();
        }
    }
}
