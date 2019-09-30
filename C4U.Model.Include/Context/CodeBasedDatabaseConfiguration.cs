using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServerCompact;
using System.Data.SqlServerCe;
using System.Text;

namespace Count4U.Model.App_Data
{
	public partial class MainDB : ObjectContext
	{
		static MainDB()
		{
			DbConfiguration.SetConfiguration(new CodeBasedDatabaseConfiguration());
		}
	}

	public class CodeBasedDatabaseConfiguration : DbConfiguration
	{
		public CodeBasedDatabaseConfiguration()
		{
			SetExecutionStrategy("System.Data.SqlServerCe.4.0", () => new DefaultExecutionStrategy());
			SetProviderFactory("System.Data.SqlServerCe.4.0", new SqlCeProviderFactory());
			SetProviderServices("System.Data.SqlServerCe.4.0", SqlCeProviderServices.Instance);
			SetProviderFactoryResolver(new CodeBasedDbProviderFactoryResolver());
		}
	}

	internal class CodeBasedDbProviderFactoryResolver : IDbProviderFactoryResolver
	{
		private readonly DbProviderFactory sqlServerCeDbProviderFactory = new SqlCeProviderFactory();

		public DbProviderFactory ResolveProviderFactory(DbConnection connection)
		{
			var connectionType = connection.GetType();
			var assembly = connectionType.Assembly;
			if (assembly.FullName.Contains("System.Data.SqlServerCe"))
			{
				return sqlServerCeDbProviderFactory;
			}
			if (assembly.FullName.Contains("EntityFramework"))
			{
				return EntityProviderFactory.Instance;
			}
			return null;
		}
	}
}
