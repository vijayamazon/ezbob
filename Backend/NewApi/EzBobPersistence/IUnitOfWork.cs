namespace EzBobPersistence {
    using System;

    public interface IUnitOfWork : IDisposable {
        void Commit();
    }
}
