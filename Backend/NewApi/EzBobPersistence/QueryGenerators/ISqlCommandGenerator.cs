namespace EzBobPersistence.QueryGenerators
{
    using System.Data.SqlClient;

    public interface ISqlCommandGenerator {
        SqlCommand GenerateCommand();
    }
}
