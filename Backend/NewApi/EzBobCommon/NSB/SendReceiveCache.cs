namespace EzBobCommon.NSB {
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Used by send-receive handlers
    /// </summary>
    public class SendReceiveCache {
        private readonly ConcurrentDictionary<Guid, object> dict = new ConcurrentDictionary<Guid, object>();

        /// <summary>
        /// Sets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="obj">The object.</param>
        public void Set(Guid id, Object obj) {
            this.dict[id] = obj;
        }

        /// <summary>
        /// Removes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Optional<object> Remove(Guid id) {
            object res;
            if (this.dict.TryRemove(id, out res)) {
                return Optional<object>.Of(res);
            }

            return Optional<object>.Empty();
        }
    }
}
