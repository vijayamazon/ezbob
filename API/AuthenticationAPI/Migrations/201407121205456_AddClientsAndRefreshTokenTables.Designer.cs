// <auto-generated />
namespace Ezbob.API.AuthenticationAPI.Migrations
{
	using System.CodeDom.Compiler;
	using System.Data.Entity.Migrations.Infrastructure;
	using System.Resources;

	[GeneratedCode("EntityFramework.Migrations", "6.1.0-30225")]
    public sealed partial class AddClientsAndRefreshTokenTables : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(AddClientsAndRefreshTokenTables));
        
        string IMigrationMetadata.Id
        {
            get { return "201407121205456_AddClientsAndRefreshTokenTables"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return this.Resources.GetString("Target"); }
        }
    }
}