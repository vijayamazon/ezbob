namespace Ezbob.API.AuthenticationAPI.Migrations
{
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Infrastructure;
	using System.Data.Entity.Migrations;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading.Tasks;
	using Ezbob.API.AuthenticationAPI.Entities;
	using Ezbob.API.AuthenticationAPI.Models;
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.EntityFramework;

	internal sealed class Configuration : DbMigrationsConfiguration<AuthContext>
    {
        public Configuration()
        {
			//Trace.WriteLine("---------------Migration Configuration ---------------------");
            AutomaticMigrationsEnabled = false;
	        //this.Seed(new AuthContext());
        }

        /*protected override void Seed(AuthContext context)
        {
            if (context.Clients.Count() > 0)
            {
                return;
            }
            context.Clients.AddRange(BuildClientsList());
            context.SaveChanges();
        }*/

		protected override void Seed(AuthContext context) {

			Trace.WriteLine("==============Seed==============");

			if (context.Clients.Count() < 2 ) {
				Trace.WriteLine("Adding clients");
				context.Clients.AddRange(BuildClientsList());
			}

			if (!context.Roles.Any()) {

				Trace.WriteLine("Adding Roles");

				var roleStore = new RoleStore<IdentityRole>(context);
				var roleManager = new RoleManager<IdentityRole>(roleStore);
				var adminRole = new IdentityRole {
					Name = "Administrator"
				};

				roleManager.Create(adminRole);

				var alibabaPartnerRole = new IdentityRole {
					Name = "PartnerAlibaba"
				};

				roleManager.Create(alibabaPartnerRole);
			}

			if (context.Users.Any() ) {
				using (AuthRepository _repo = new AuthRepository()) {
					var alibabaUser = _repo.FindByUserName("partherAppAlibaba");
					Task<bool> userToRole = _repo.AddToRole(alibabaUser.Id.ToString(), "PartnerAlibaba");
					Trace.WriteLine("Adding userToRole partherAppAlibaba=> PartnerAlibaba");
				}
			}

			try {
				context.SaveChanges();
			} catch (DbUpdateException dbUpdateException) {

				Trace.WriteLine(string.Format("DbUpdateException white seed Identity tables; {0} ", dbUpdateException));
			} catch (Exception e) {
				Trace.WriteLine(string.Format("Exception white seed Identity tables; {0} ", e));
			}
		}




        private static List<Client> BuildClientsList()
        {
            List<Client> ClientsList = new List<Client> 
            {
                new Client
                { Id = "ngAuthApp", 
                    Secret= Helper.GetHash("abc@123"), 
                    Name="AngularJS front-end Application", 
                    ApplicationType =  ApplicationTypes.JavaScript, 
                    Active = true, 
                    RefreshTokenLifeTime = 7200, 
                    AllowedOrigin = "https://localhost:44302"   
                },
                new Client
                { Id = "consoleApp", 
                    Secret=Helper.GetHash("123@abc"), 
                    Name="Console Application", 
                    ApplicationType = ApplicationTypes.NativeConfidential, 
                    Active = true, 
                    RefreshTokenLifeTime = 14400, 
                    AllowedOrigin = "*"
                },
				 new Client
                {	Id = "pAliServer7c60C021e70B", 
                    Secret= Helper.GetHash("152863423315581"), 
                    Name="Alibaba partner server application", 
                    ApplicationType = ApplicationTypes.NativeConfidential, 
                    Active = true, 
                    RefreshTokenLifeTime = 14400, 
                    AllowedOrigin = "*"  
                },
				 new Client
                {	Id = "pAliClient86f35Fd2896", 
                    Secret= Helper.GetHash("352878968536372"), 
                    Name="Alibaba partner client application", 
					ApplicationType =  ApplicationTypes.JavaScript, 
                    Active = true, 
                    RefreshTokenLifeTime = 7200, 
                    AllowedOrigin = "https://localhost:44302"   
                },

            };

            return ClientsList;
        }
    }
}
