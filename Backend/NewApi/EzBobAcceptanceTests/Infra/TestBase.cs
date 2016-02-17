using System.Threading.Tasks;

namespace EzBobAcceptanceTests.Infra {
    public class TestBase {
        protected static Task<T> CreateCompletedTask<T>(T result) {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResult(result);
            return taskSource.Task;
        }
    }
}
