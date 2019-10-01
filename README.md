# dotnetcore_mssqlce
Try work with dotnetcore and mssqlce Without  app.config !

dotnet core 3 + EF 6.3 + MSSQL CE  + .edmx(EF6 metadata)
Without  app.config !

Sample DB -MainDB.sdf.
Sample Context -MainDB.edmx.

Loot at for explanation steps
https://github.com/parad74/dotnetcore_mssqlce/commit/beebf90f1e252f64073794c5a55671735eed906c#diff-d9f32fc1395f6c8f94b824c308bd4271

1.	First Idea I got from https://github.com/efcore/EdmxDotNetCoreSample/ 
This sample shows a way to work with edmx (EF6 metadata) in a .NET Core project in Visual Studio without designer support.
It uses a .NET Framework project to host the edmx file, which is supported by the designer, and then imports the edmx file and the relevant generated entity and DbContext classes as linked files in the .NET Core project.

That way you can use the designer to visualize and modify the model using the designer, getting any changes automatically impact the generated code using the regular T4 templates, and then use the results from a .NET Core application.

You will need to add new linked files for any new entity classes you add or rename in the designer.

In project C4U.Model.Include.csproj 30 - 33 lines

EntityDeploy Include="..\4.8\C4U.Model\App_Data\MainDB.edmx" Link="App_Data\MainDB.edmx"

Compile Include="..\4.8\C4U.Model\App_Data\Model6.cs" Link="App_Data\Model6.cs"


2.	In projects C4U.Model.Include/app.config  and host/app.config delete all providers.

3.	To project C4U.Model.Include  copy folder  amd64 from your .Net project
with files 
sqlceca40.dll
sqlcecompact40.dll
sqlceer40EN.dll
sqlceme40.dll
sqlceqp40.dll
sqlcese40.dll

In project file C4U.Model.Include/C4U.Model.Include.csproj  will add Content Include s 7-16 lines

4.	Edit C4U.Model.Include/C4U.Model.Include.csproj project file. Add Folder Include="amd64\" 60 line

To include sqlce*.dll to result folder

5.	Add classes to C4U.Model.Include  project
 C4U.Model.Include/Context/CodeBasedDatabaseConfiguration.cs 
 
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
  
6.	How to use. host/Program.cs  To run with ADO 

		DbProviderFactories.RegisterFactory("System.Data.SqlServerCe.4.0", new SqlCeProviderFactory());
			var pp = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
			//classes = DbProviderFactories.GetFactoryClasses();
			string path = Path.GetFullPath(@"../C4U.Model.Include/App_Data");

			var cs = @"metadata=res://*/App_Data.MainDB.csdl|res://*/App_Data.MainDB.ssdl|res://*/App_Data.MainDB.msl;provider=System.Data.SqlServerCe.4.0;provider connection string='Data Source =T:\Count4U\trunk\github\Count4U\C4U.Model.Include\App_Data\MainDB.sdf'";

			using (Count4U.Model.App_Data.MainDB dc = new Count4U.Model.App_Data.MainDB(cs))
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

7.	How to use. host/Program.cs  To run with ObjectContext

	
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

