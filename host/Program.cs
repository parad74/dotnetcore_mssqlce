using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

			var cs = @"metadata=res://*/App_Data.MainDB.csdl|res://*/App_Data.MainDB.ssdl|res://*/App_Data.MainDB.msl;provider=System.Data.SqlServerCe.4.0;provider connection string='Data Source =T:\Count4U\trunk\github\Count4U\C4U.Model.Include\App_Data\MainDB.sdf'";
		
			using (Count4U.Model.App_Data.MainDB dc = new Count4U.Model.App_Data.MainDB(cs))
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
