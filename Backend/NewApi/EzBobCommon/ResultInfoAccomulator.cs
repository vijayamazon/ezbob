namespace EzBobCommon {
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ResultInfoAccomulator<T> : InfoAccumulator {
        public ResultInfoAccomulator() {
            if (!typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T))))
                throw new InvalidOperationException("A serializable Type is required");
        }

        public T Result { get; set; }
    }
}
