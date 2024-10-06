using Beep.Vis.Module;
using TheTechIdea.Beep;
using TheTechIdea.Util;
using DataManagementModels.DriversConfigurations;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using System.Data;
using DataManagementModels.DataBase;
using TheTechIdea;
namespace Beep.InMemory.Logic
{
    public static class BeepInMemoryManager
    {
        // Holds the name of the current in-memory database
        public static string CurrentDbName { get; set; }

        // Creates an in-memory database connection
        public static ConnectionProperties CreateInMemoryDB(IDMEEditor DMEEditor, IVisManager Vis)
        {
            ConnectionProperties conn = null;
            try
            {
                // Get the available in-memory database classes
                List<AssemblyClassDefinition> InMemoryDBs = GetInMemoryDBs(DMEEditor);
                string dbname = "";
                string classhandle = "";
                List<string> ls = InMemoryDBs.Select(p => p.className).ToList();

                // Prompt user to select an in-memory database provider
                if (Vis.Controlmanager.InputComboBox("Beep", "Select InMemoryDB Provider", ls, ref classhandle) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(classhandle))
                    {
                        // Prompt user to enter a name for the database
                        if (Vis.Controlmanager.InputBox("Beep", "Enter name for Database", ref dbname) == DialogResult.OK)
                        {
                            if (!string.IsNullOrEmpty(dbname))
                            {
                                // Create the connection properties
                                conn = CreateConn(DMEEditor, dbname, classhandle);
                                if (conn != null)
                                {
                                    // Add the connection to the configuration and save it
                                    DMEEditor.ConfigEditor.DataConnections.Add(conn);
                                    DMEEditor.ConfigEditor.SaveDataconnectionsValues();
                                    DMEEditor.AddLogMessage("Beep", "Create Connection Successfully", DateTime.Now, -1, "", Errors.Ok);
                                }
                                else
                                {
                                    DMEEditor.AddLogMessage("Beep", "Could not Create Connection", DateTime.Now, -1, "", Errors.Failed);
                                }
                            }
                        }
                    }
                }
                // Store the name of the current database
                CurrentDbName = dbname;
                return conn;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during creation
                DMEEditor.AddLogMessage("Beep", $"Could not Create Connection {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
            return conn;
        }

        // Retrieves a list of available in-memory database classes
        public static List<AssemblyClassDefinition> GetInMemoryDBs(IDMEEditor DMEEditor)
        {
            return DMEEditor.ConfigEditor.DataSourcesClasses.Where(p => p.classProperties != null && p.InMemory == true).ToList();
        }

        // Creates driver configuration for the in-memory database
        public static ConnectionDriversConfig CreateDriverConfig(IDMEEditor DMEEditor, string dbname, string pclassname)
        {
            try
            {
                // Find the appropriate driver configuration
                AssemblyClassDefinition assembly = GetInMemoryDBs(DMEEditor).Where(p => p.className == pclassname).FirstOrDefault();
                ConnectionDriversConfig package = DMEEditor.ConfigEditor.DataDriversClasses.Where(x => x.classHandler == pclassname).OrderByDescending(o => o.version).FirstOrDefault();

                return package;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during driver configuration creation
                DMEEditor.AddLogMessage("Beep", $"Could not Create Drivers Config {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
                return null;
            }
        }

        // Creates the connection properties for the in-memory database
        public static ConnectionProperties CreateConn(IDMEEditor DMEEditor, string dbname, string pclassname)
        {
            try
            {
                ConnectionProperties dataConnection = new ConnectionProperties();
                ConnectionDriversConfig package = CreateDriverConfig(DMEEditor, dbname, pclassname);

                if (package != null)
                {
                    // Set the connection properties for an in-memory database
                    dataConnection.Category = DatasourceCategory.INMEMORY;
                    dataConnection.DatabaseType = package.DatasourceType;
                    dataConnection.ConnectionName = dbname;
                    dataConnection.DriverName = package.PackageName;
                    dataConnection.DriverVersion = package.version;
                    dataConnection.ID = DMEEditor.ConfigEditor.DataConnections.Max(y => y.ID) + 1;
                    dataConnection.Database = dbname;
                    dataConnection.IsInMemory = true;
                    dataConnection.IsLocal = true;
                    dataConnection.DriverName = package.PackageName;
                    dataConnection.DriverVersion = package.version;
                    dataConnection.ConnectionString = package.ConnectionString;
                    return dataConnection;
                }
                else
                {
                    // Log if no driver configuration was found
                    DMEEditor.AddLogMessage("Beep", $"Could not Find Drivers Config {pclassname}", DateTime.Now, -1, "", Errors.Failed);
                    return null;
                }
            }
            catch (Exception)
            {
                // Log any errors that occur during connection creation
                DMEEditor.AddLogMessage("Beep", $"Could not Find Drivers Config {pclassname}", DateTime.Now, -1, "", Errors.Failed);
                return null;
            }
        }

        // Loads the structure of the in-memory database from a file
        public static IErrorsInfo LoadStructure(IDMEEditor DMEEditor, IDataSource ds, string dbpath, IVisManager Vis)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                IInMemoryDB inds = (IInMemoryDB)ds;
                string filepath = Path.Combine(dbpath, "createscripts.json");
                string InMemoryStructuresfilepath = Path.Combine(dbpath, "InMemoryStructures.json");
                ds.ConnectionStatus = ConnectionState.Open;
                inds.InMemoryStructures = new List<EntityStructure>();
                ds.Entities = new List<EntityStructure>();
                ds.EntitiesNames = new List<string>();
                CancellationTokenSource token = new CancellationTokenSource();

                // Load in-memory structures if they exist
                if (File.Exists(InMemoryStructuresfilepath))
                {
                    inds.InMemoryStructures = DMEEditor.ConfigEditor.JsonLoader.DeserializeObject<EntityStructure>(InMemoryStructuresfilepath);
                }

                // Load create scripts if they exist
                if (File.Exists(filepath))
                {
                    var hdr = DMEEditor.ConfigEditor.JsonLoader.DeserializeSingleObject<ETLScriptHDR>(filepath);
                    DMEEditor.ETL.Script = hdr;
                    DMEEditor.ETL.Script.LastRunDateTime = System.DateTime.Now;
                    PassedArgs args = new PassedArgs();
                    args.Messege = $"Loadin InMemory Structure {ds.DatasourceName}";
                    Vis.ShowWaitForm(args);
                    Vis.PasstoWaitForm(args);
                    DMEEditor.progress = new Progress<PassedArgs>(percent => {
                        Vis.PasstoWaitForm(args);
                    });
                    DMEEditor.ETL.RunCreateScript(DMEEditor.progress, token.Token);
                    Vis.CloseWaitForm();
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during structure loading
                DMEEditor.AddLogMessage("Beep", $"Could not Load InMemory Structure for {ds.DatasourceName}- {ex.Message}", System.DateTime.Now, 0, null, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        // Saves the structure of the in-memory database to a file
        public static IErrorsInfo SaveStructure(IDMEEditor DMEEditor, IDataSource ds, string dbpath)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                IInMemoryDB inds = (IInMemoryDB)ds;
                if (inds.InMemoryStructures.Count > 0)
                {
                    // Create directory for saving files if it does not exist
                    Directory.CreateDirectory(dbpath);
                    string filepath = Path.Combine(dbpath, "createscripts.json");
                    string InMemoryStructuresfilepath = Path.Combine(dbpath, "InMemoryStructures.json");

                    // Create and serialize the ETL script header
                    ETLScriptHDR scriptHDR = new ETLScriptHDR();
                    scriptHDR.ScriptDTL = new List<ETLScriptDet>();
                    CancellationTokenSource token = new CancellationTokenSource();
                    scriptHDR.scriptName = ds.Dataconnection.ConnectionProp.Database;
                    scriptHDR.scriptStatus = "SAVED";
                    scriptHDR.ScriptDTL.AddRange(DMEEditor.ETL.GetCreateEntityScript(ds, inds.InMemoryStructures, DMEEditor.progress, token.Token));
                    scriptHDR.ScriptDTL.AddRange(DMEEditor.ETL.GetCopyDataEntityScript(ds, inds.InMemoryStructures, DMEEditor.progress, token.Token));
                    DMEEditor.ConfigEditor.JsonLoader.Serialize(filepath, scriptHDR);
                    DMEEditor.ConfigEditor.JsonLoader.Serialize(InMemoryStructuresfilepath, inds.InMemoryStructures);
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during structure saving
                DMEEditor.AddLogMessage("Beep", $"Could not save InMemory Structure for {ds.DatasourceName}- {ex.Message}", System.DateTime.Now, 0, null, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
    }
}