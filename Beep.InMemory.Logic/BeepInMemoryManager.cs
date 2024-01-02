using BeepEnterprize.Vis.Module;
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
        public static string CurrentDbName { get; set; }
        public static ConnectionProperties CreateInMemoryDB(IDMEEditor DMEEditor, IVisManager Vis)
        {
            ConnectionProperties conn = null;
            try
            {
               
                List<AssemblyClassDefinition> InMemoryDBs = GetInMemoryDBs(DMEEditor);
                string dbname = "";
                string classhandle = "";
                List<string> ls=InMemoryDBs.Select(p => p.className).ToList();
                if(Vis.Controlmanager.InputComboBox("Beep","Select InMemoryDB Provider",ls,ref classhandle) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(classhandle))
                    {
                        if (Vis.Controlmanager.InputBox("Beep", "Enter name for Database", ref dbname) == DialogResult.OK)
                        {
                            if (!string.IsNullOrEmpty(dbname))
                            {
                                conn = CreateConn(DMEEditor, dbname, classhandle);
                                if (conn != null)
                                {
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
                CurrentDbName= dbname;
                return conn;
              
            }
            catch (Exception ex)
            {

                DMEEditor.AddLogMessage("Beep", $"Could not Create Connection {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
            }
            return conn;
        }
        public static List<AssemblyClassDefinition> GetInMemoryDBs(IDMEEditor DMEEditor)
        {
            return DMEEditor.ConfigEditor.DataSourcesClasses.Where(p => p.classProperties!=null &&  p.InMemory==true).ToList();
        }
        public static ConnectionDriversConfig CreateDriverConfig(IDMEEditor DMEEditor, string dbname, string pclassname)
        {
            try
            {
                AssemblyClassDefinition assembly= GetInMemoryDBs(DMEEditor).Where(p => p.className == pclassname).FirstOrDefault();
                ConnectionDriversConfig package =  DMEEditor.ConfigEditor.DataDriversClasses.Where(x => x.classHandler == pclassname).OrderByDescending(o => o.version).FirstOrDefault();
              
                return package;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Beep", $"Could not Create Drivers Config {ex.Message}", DateTime.Now, -1, "", Errors.Failed);
                return null;
            }
        }
        public static ConnectionProperties CreateConn(IDMEEditor DMEEditor,string dbname,string pclassname)
        {
            try
            {

                ConnectionProperties dataConnection = new ConnectionProperties();
                ConnectionDriversConfig package = CreateDriverConfig(DMEEditor, dbname, pclassname);

                if (package!=null)
                {
                    dataConnection.Category = DatasourceCategory.INMEMORY;//(DatasourceCategory)(int) Enum.Parse(typeof( DatasourceCategory),CategorycomboBox.Text);
                    dataConnection.DatabaseType = package.DatasourceType; //(DataSourceType)(int)Enum.Parse(typeof(DataSourceType), DatabaseTypecomboBox.Text);
                    dataConnection.ConnectionName = dbname;
                    dataConnection.DriverName = package.PackageName;
                    dataConnection.DriverVersion = package.version;
                    dataConnection.ID = DMEEditor.ConfigEditor.DataConnections.Max(y => y.ID) + 1;
                    dataConnection.Database = dbname;
                    dataConnection.IsInMemory = true;
                    dataConnection.IsLocal = true;
                    dataConnection.DriverName = package.PackageName;
                    dataConnection.DriverVersion = package.version;
                    
                    dataConnection.ConnectionString = package.ConnectionString; //Path.Combine(dataConnection.FilePath, dataConnection.FileName);
                    return dataConnection;

                }
                else
                {
                    DMEEditor.AddLogMessage("Beep", $"Could not Find Drivers Config {pclassname}", DateTime.Now, -1, "", Errors.Failed);
                    return null;
                }
                    

            }
            catch (Exception)
            {
              
                DMEEditor.AddLogMessage("Beep", $"Could not Find Drivers Config {pclassname}", DateTime.Now, -1, "", Errors.Failed);
                return null;
            }
        }
        public static IErrorsInfo LoadStructure(IDMEEditor DMEEditor,IDataSource ds,string dbpath, IVisManager Vis)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                IInMemoryDB inds= (IInMemoryDB)ds;
                string filepath = Path.Combine(dbpath, "createscripts.json");
                string InMemoryStructuresfilepath = Path.Combine(dbpath, "InMemoryStructures.json");
                ds.ConnectionStatus = ConnectionState.Open;
                inds.InMemoryStructures = new List<EntityStructure>();
                ds.Entities = new List<EntityStructure>();
                ds.EntitiesNames = new List<string>();
                CancellationTokenSource token = new CancellationTokenSource();
                if (File.Exists(InMemoryStructuresfilepath))
                {
                    inds.InMemoryStructures = DMEEditor.ConfigEditor.JsonLoader.DeserializeObject<EntityStructure>(InMemoryStructuresfilepath);
                }
                if (File.Exists(filepath))
                {
                    var hdr = DMEEditor.ConfigEditor.JsonLoader.DeserializeSingleObject<ETLScriptHDR>(filepath);
                    DMEEditor.ETL.Script = hdr;
                    DMEEditor.ETL.Script.LastRunDateTime = System.DateTime.Now;
                    PassedArgs args=new PassedArgs();
                    args.Messege= $"Loadin InMemory Structure {ds.DatasourceName}";
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
                DMEEditor.AddLogMessage("Beep", $"Could not Load InMemory Structure for {ds.DatasourceName}- {ex.Message}", System.DateTime.Now, 0, null, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
        public static IErrorsInfo SaveStructure(IDMEEditor DMEEditor, IDataSource ds, string dbpath)
        {
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            try
            {
                IInMemoryDB inds = (IInMemoryDB)ds;
                if (inds.InMemoryStructures.Count > 0)
                {
                    Directory.CreateDirectory(dbpath);
                    string filepath = Path.Combine(dbpath, "createscripts.json");
                    string InMemoryStructuresfilepath = Path.Combine(dbpath, "InMemoryStructures.json");
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
                DMEEditor.AddLogMessage("Beep", $"Could not save InMemory Structure for {ds.DatasourceName}- {ex.Message}", System.DateTime.Now, 0, null, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
    }
}
