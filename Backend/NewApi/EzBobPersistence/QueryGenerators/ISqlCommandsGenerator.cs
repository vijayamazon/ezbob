using System.Collections.Generic;

namespace EzBobPersistence.QueryGenerators
{
    using System.Data.SqlClient;

    interface ISqlCommandsGenerator {
        IEnumerable<SqlCommand> GenerateCommands();
    }
}
