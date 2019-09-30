# dotnetcore_mssqlce
Try work with dotnetcore and mssqlce Without  app.config !

dotnet core 3 + EF 6.3 + MSSQL CE  + .edmx(EF6 metadata)
Without  app.config !

Sample DB -MainDB.sdf.
Sample Context -MainDB.edmx.

1.	First Idea I got from https://github.com/efcore/EdmxDotNetCoreSample/ 
This sample shows a way to work with edmx (EF6 metadata) in a .NET Core project in Visual Studio without designer support.
It uses a .NET Framework project to host the edmx file, which is supported by the designer, and then imports the edmx file and the relevant generated entity and DbContext classes as linked files in the .NET Core project.

That way you can use the designer to visualize and modify the model using the designer, getting any changes automatically impact the generated code using the regular T4 templates, and then use the results from a .NET Core application.

You will need to add new linked files for any new entity classes you add or rename in the designer.

In project dotnet Core it looks so.
<ItemGroup>
		<EntityDeploy Include="..\4.8\C4U.Model\App_Data\MainDB.edmx" Link="App_Data\MainDB.edmx" />
		<Compile Include="..\4.8\C4U.Model\App_Data\Model6.cs" Link="App_Data\Model6.cs" />
	</ItemGroup>

2.	In project dotnet Core delete from app.config all providers.

3.	To project dotnet Core copy folder  amd64 from your .Net project
with files 
sqlceca40.dll">
sqlcecompact40.dll">
sqlceer40EN.dll">
sqlceme40.dll">
sqlceqp40.dll">
sqlcese40.dll">

In project file will add
<ItemGroup>
	  <Content Include="amd64\sqlceca40.dll">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	  <Content Include="amd64\sqlcecompact40.dll">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	  <Content Include="amd64\sqlceer40EN.dll">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	  <Content Include="amd64\sqlceme40.dll">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	  <Content Include="amd64\sqlceqp40.dll">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	  <Content Include="amd64\sqlcese40.dll">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	  <Content Include="App_Data\MainDB.sdf">
	    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
	  </Content>
	</ItemGroup> 

4.	Edit project dotnet core 3 file. Add
<ItemGroup>
	<Folder Include="amd64\" />
</ItemGroup>
To include sqlce*.dll to result folder

5.	Add classes to dotnet Core 3 project

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
  
6.	How to use. To run with ADO 
		DbProviderFactories.RegisterFactory("System.Data.SqlServerCe.4.0", new SqlCeProviderFactory());
			var pp = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
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


7.	How to use. To run with ObjectContext
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
