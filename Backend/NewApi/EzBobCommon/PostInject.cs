using System;

namespace EzBobCommon {
    /// <summary>
    /// This attribute should be used on methods to be automatically called immediately after instance's dependencies where injected
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PostInject : Attribute {}
}
