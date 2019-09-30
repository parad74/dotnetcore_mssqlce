using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.SqlServerCompact;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Count4U.Model.App_Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace host
{
	public class Program
	{
		public static void Main(string[] args)
		{
	 		//	var classes = DbProviderFactories.GetFactoryClasses();
			// workaround:
			//DbProviderFactories.RegisterFactory("System.Data.SqlServerCe.4.0", SqlClientFactory.Instance);
			DbProviderFactories.RegisterFactory("System.Data.SqlServerCe.4.0", new SqlCeProviderFactory());
			var pp = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
			//classes = DbProviderFactories.GetFactoryClasses();
			string path = Path.GetFullPath(@"../C4U.Model.Include/App_Data");

			//ADO - work!!!
			using (DbConnection connection = pp.CreateConnection())       //System.Data.SqlServerCe.SqlCeConnection
			{
				connection.ConnectionString = @"Data Source = " + path + @"\MainDB.sdf";
				connection.Open();
				using (DbCommand command = connection.CreateCommand())
				{
					command.CommandText = "select * from [Customer]	";
					command.CommandType = CommandType.Text;

					using (DbDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var result = reader.GetValue(1);
							if (!reader.IsDBNull(0))
							{
								var customer = result;
							}
						}
					}
				}
			}

			//ObjectContext	   work!!!
			var cs = @"metadata=res://*/App_Data.MainDB.csdl|res://*/App_Data.MainDB.ssdl|res://*/App_Data.MainDB.msl;provider=System.Data.SqlServerCe.4.0;provider connection string='Data Source =" + path + @"\MainDB.sdf'";

			using (MainDB dc = new MainDB(cs))
			{
				try
				{
					var entertis = dc.Customer;
					var entertiList = entertis.ToList();
				}
				catch (Exception ext)
				{
					string message = ext.Message;
				}
			}

			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
