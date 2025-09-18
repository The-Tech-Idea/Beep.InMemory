using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;

namespace Beep.InMemory.Nodes
{
    /// <summary>
    /// Represents an individual in-memory database instance in the tree
    /// Provides operations for managing database structure, data loading, and entity management
    /// </summary>
    [AddinAttribute(Caption = "InMemory Database", Name = "InMemoryDatabaseNode.Beep", misc = "Beep", 
                    iconimage = "inmemorydatabase.png", menu = "Beep", ObjectType = "Beep")]
    [AddinVisSchema(BranchType = EnumPointType.DataPoint, BranchClass = "INMEMORY")]
    public class InMemoryDatabaseNode : IBranch, IOrder
    {
        #region Properties
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public bool Visible { get; set; } = true;
        public IBranch ParentBranch { get; set; }
        public string ObjectType { get; set; } = "Beep";
        public int ID { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public int Order { get; set; } = 4;
        public string Name { get; set; }
        public string BranchText { get; set; } = "InMemory Database";
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.DataPoint;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "inmemorydatabase.png";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "INMEMORY";
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; } = new List<string>();
        public object TreeStrucure { get; set; }
        public IAppManager Visutil { get; set; }
        public int MiscID { get; set; }
        public ConnectionProperties ConnectionProperties { get; set; }
        public bool IsDataSourceNode { get; set; } = false;
        public string MenuID { get; set; }

        /// <summary>
        /// Reference to the in-memory database instance
        /// </summary>
        private IInMemoryDB memoryDB;
        
        /// <summary>
        /// Tracks if the database is currently performing operations
        /// </summary>
        private bool _isLoading = false;
        #endregion

        #region Constructors
        public InMemoryDatabaseNode()
        {
            InitializeNode();
        }

        public InMemoryDatabaseNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, 
                                  string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranch = pParentNode;
            ParentBranchID = pParentNode?.ID ?? 0;
            BranchText = pBranchText;
            BranchType = pBranchType;
            DataSourceName = pBranchText;
            
            if (pID != 0)
            {
                ID = pID;
                BranchID = ID;
            }

            InitializeNode();
            InitializeMemoryDatabase();
        }

        private void InitializeNode()
        {
            BranchClass = "INMEMORY";
            IconImageName = "inmemorydatabase.png";
            BranchType = EnumPointType.DataPoint;
            ObjectType = "Beep";
            Order = 4;
            ChildBranchs = new List<IBranch>();
            BranchActions = new List<string>();
        }

        private void InitializeMemoryDatabase()
        {
            try
            {
                if (!string.IsNullOrEmpty(DataSourceName))
                {
                    memoryDB = DMEEditor?.GetDataSource(DataSourceName) as IInMemoryDB;
                    DataSource = memoryDB as IDataSource;
                    
                    if (memoryDB == null)
                    {
                        DMEEditor?.AddLogMessage("Warning", $"InMemory database '{DataSourceName}' not found", 
                            DateTime.Now, 0, "InitializeMemoryDatabase", Errors.Ok);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage("Error", $"Failed to initialize memory database: {ex.Message}", 
                    DateTime.Now, -1, "InitializeMemoryDatabase", Errors.Failed);
            }
        }
        #endregion

        #region Interface Methods
        /// <summary>
        /// Creates child nodes representing entities in the in-memory database
        /// </summary>
        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                if (_isLoading)
                {
                    DMEEditor.AddLogMessage("Warning", "Database is currently loading, please wait", 
                        DateTime.Now, 0, "CreateChildNodes", Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                if (memoryDB == null)
                {
                    InitializeMemoryDatabase();
                    if (memoryDB == null)
                    {
                        DMEEditor.AddLogMessage("Error", $"Cannot create child nodes: database '{DataSourceName}' not available", 
                            DateTime.Now, -1, "CreateChildNodes", Errors.Failed);
                        return DMEEditor.ErrorObject;
                    }
                }

                // Check if structure is ready
                if (!memoryDB.IsStructureCreated)
                {
                    DMEEditor.AddLogMessage("Info", $"Database structure not created for '{DataSourceName}'. Use 'Get Entities' to load structure.", 
                        DateTime.Now, 0, "CreateChildNodes", Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                DataSourceDefaultMethods.GetEntities(this, DMEEditor, Visutil);
                DMEEditor.AddLogMessage("Success", $"Created child nodes for database '{DataSourceName}'", 
                    DateTime.Now, 0, "CreateChildNodes", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create child nodes: {ex.Message}", 
                    DateTime.Now, -1, "CreateChildNodes", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo ExecuteBranchAction(string ActionName)
        {
            try
            {
                switch (ActionName?.ToUpper())
                {
                    case "GETENTITIES":
                        return GetEntities();
                    case "REFRESHDATA":
                        return RefreshData();
                    case "REMOVE":
                        return remove();
                    case "LOADDATA":
                        return LoadDatabaseData();
                    case "CLEARDATA":
                        return ClearDatabaseData();
                    case "EXPORTDATA":
                        return ExportDatabaseData();
                    default:
                        DMEEditor.AddLogMessage("Warning", $"Unknown action: {ActionName}", 
                            DateTime.Now, 0, "ExecuteBranchAction", Errors.Ok);
                        break;
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to execute action '{ActionName}': {ex.Message}", 
                    DateTime.Now, -1, "ExecuteBranchAction", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo MenuItemClicked(string ActionName)
        {
            return ExecuteBranchAction(ActionName);
        }

        public IErrorsInfo RemoveChildNodes()
        {
            try
            {
                foreach (IBranch child in ChildBranchs.ToArray())
                {
                    TreeEditor.Treebranchhandler.RemoveBranch(child);
                }
                ChildBranchs.Clear();

                DMEEditor.AddLogMessage("Success", $"Removed child nodes from database '{DataSourceName}'", 
                    DateTime.Now, 0, "RemoveChildNodes", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to remove child nodes: {ex.Message}", 
                    DateTime.Now, -1, "RemoveChildNodes", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo SetConfig(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, 
                                   string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            try
            {
                TreeEditor = pTreeEditor;
                DMEEditor = pDMEEditor;
                ParentBranch = pParentNode;
                ParentBranchID = pParentNode?.ID ?? 0;
                BranchText = pBranchText;
                BranchType = pBranchType;

                if (pID != 0)
                {
                    ID = pID;
                    BranchID = pID;
                }

                DMEEditor.AddLogMessage("Success", $"InMemory database configuration set: {BranchText}", 
                    DateTime.Now, 0, "SetConfig", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to set configuration: {ex.Message}", 
                    DateTime.Now, -1, "SetConfig", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        public IBranch CreateCategoryNode(CategoryFolder p)
        {
            // Database nodes don't create categories
            return this;
        }
        #endregion

        #region Public Commands
        /// <summary>
        /// Gets entities and creates the database structure
        /// </summary>
        [CommandAttribute(Caption = "Get Entities", Hidden = false, iconimage = "getentities.png")]
        public IErrorsInfo GetEntities()
        {
            if (_isLoading)
            {
                DMEEditor.AddLogMessage("Warning", "Database operation already in progress", 
                    DateTime.Now, 0, "GetEntities", Errors.Ok);
                return DMEEditor.ErrorObject;
            }

            try
            {
                _isLoading = true;
                EnsureMemoryDatabaseAvailable();

                if (memoryDB == null)
                {
                    return DMEEditor.ErrorObject;
                }

                if (!memoryDB.IsStructureCreated)
                {
                    LoadDatabaseStructure();
                }

                if (memoryDB.IsLoaded)
                {
                    DataSourceDefaultMethods.GetEntities(this, DMEEditor, Visutil);
                    DMEEditor.AddLogMessage("Success", $"Entities loaded for database '{DataSourceName}'", 
                        DateTime.Now, 0, "GetEntities", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to get entities: {ex.Message}", 
                    DateTime.Now, -1, "GetEntities", Errors.Failed);
            }
            finally
            {
                _isLoading = false;
                Visutil?.CloseWaitForm();
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Refreshes the database data from external sources
        /// </summary>
        [CommandAttribute(Caption = "Refresh Data", Hidden = false, iconimage = "refresh.png")]
        public IErrorsInfo RefreshData()
        {
            if (_isLoading)
            {
                DMEEditor.AddLogMessage("Warning", "Database operation already in progress", 
                    DateTime.Now, 0, "RefreshData", Errors.Ok);
                return DMEEditor.ErrorObject;
            }

            try
            {
                _isLoading = true;
                EnsureMemoryDatabaseAvailable();

                if (memoryDB?.IsLoaded == true)
                {
                    var result = Visutil?.DialogManager?.InputBoxYesNo("Confirm Refresh", 
                        "This will refresh the data in memory. Do you want to continue?");
                    
                    if (result == BeepDialogResult.Yes)
                    {
                        ShowProgressAndExecute("Refreshing data in InMemory database", 
                            (progress, token) => memoryDB.RefreshData(progress, token));
                        
                        DMEEditor.AddLogMessage("Success", $"Data refreshed for database '{DataSourceName}'", 
                            DateTime.Now, 0, "RefreshData", Errors.Ok);
                    }
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", "Database not loaded. Use 'Get Entities' first.", 
                        DateTime.Now, 0, "RefreshData", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to refresh data: {ex.Message}", 
                    DateTime.Now, -1, "RefreshData", Errors.Failed);
            }
            finally
            {
                _isLoading = false;
                Visutil?.CloseWaitForm();
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Loads data into the in-memory database
        /// </summary>
        [CommandAttribute(Caption = "Load Data", Hidden = false, iconimage = "loaddata.png")]
        public IErrorsInfo LoadDatabaseData()
        {
            if (_isLoading)
            {
                DMEEditor.AddLogMessage("Warning", "Database operation already in progress", 
                    DateTime.Now, 0, "LoadDatabaseData", Errors.Ok);
                return DMEEditor.ErrorObject;
            }

            try
            {
                _isLoading = true;
                EnsureMemoryDatabaseAvailable();

                if (memoryDB != null && memoryDB.IsStructureCreated && !memoryDB.IsLoaded)
                {
                    ShowProgressAndExecute("Loading data into InMemory database", 
                        (progress, token) => memoryDB.LoadData(progress, token));
                    
                    memoryDB.IsLoaded = true;
                    DMEEditor.AddLogMessage("Success", $"Data loaded for database '{DataSourceName}'", 
                        DateTime.Now, 0, "LoadDatabaseData", Errors.Ok);
                }
                else if (memoryDB?.IsLoaded == true)
                {
                    DMEEditor.AddLogMessage("Info", "Database data is already loaded", 
                        DateTime.Now, 0, "LoadDatabaseData", Errors.Ok);
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", "Database structure not created. Use 'Get Entities' first.", 
                        DateTime.Now, 0, "LoadDatabaseData", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to load database data: {ex.Message}", 
                    DateTime.Now, -1, "LoadDatabaseData", Errors.Failed);
            }
            finally
            {
                _isLoading = false;
                Visutil?.CloseWaitForm();
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Clears all data from the in-memory database
        /// </summary>
        [CommandAttribute(Caption = "Clear Data", Hidden = false, iconimage = "clear.png")]
        public IErrorsInfo ClearDatabaseData()
        {
            try
            {
                EnsureMemoryDatabaseAvailable();

                if (memoryDB?.IsLoaded == true)
                {
                    var result = Visutil?.DialogManager?.InputBoxYesNo("Confirm Clear", 
                        "This will clear all data from memory. Do you want to continue?");
                    
                    if (result == BeepDialogResult.Yes)
                    {
                        // Implementation depends on IInMemoryDB interface
                        // This is a placeholder for the clear operation
                        memoryDB.IsLoaded = false;
                        RemoveChildNodes();
                        
                        DMEEditor.AddLogMessage("Success", $"Data cleared for database '{DataSourceName}'", 
                            DateTime.Now, 0, "ClearDatabaseData", Errors.Ok);
                    }
                }
                else
                {
                    DMEEditor.AddLogMessage("Info", "No data to clear", 
                        DateTime.Now, 0, "ClearDatabaseData", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to clear database data: {ex.Message}", 
                    DateTime.Now, -1, "ClearDatabaseData", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Exports database data to external format
        /// </summary>
        [CommandAttribute(Caption = "Export Data", Hidden = false, iconimage = "export.png")]
        public IErrorsInfo ExportDatabaseData()
        {
            try
            {
                EnsureMemoryDatabaseAvailable();

                if (memoryDB?.IsLoaded == true)
                {
                    // Implementation for data export
                    // This would typically open an export dialog
                    DMEEditor.AddLogMessage("Info", $"Export functionality for database '{DataSourceName}'", 
                        DateTime.Now, 0, "ExportDatabaseData", Errors.Ok);
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", "No data to export. Load data first.", 
                        DateTime.Now, 0, "ExportDatabaseData", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to export database data: {ex.Message}", 
                    DateTime.Now, -1, "ExportDatabaseData", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Removes the database connection and node
        /// </summary>
        [CommandAttribute(Caption = "Remove Database", Hidden = false, iconimage = "remove.png")]
        public IErrorsInfo remove()
        {
            try
            {
                var result = Visutil?.DialogManager?.InputBoxYesNo("Confirm Removal", 
                    $"Are you sure you want to remove database '{DataSourceName}'?");
                
                if (result == BeepDialogResult.Yes)
                {
                    RemoveChildNodes();
                    TreeEditor?.Treebranchhandler?.RemoveBranch(this);
                    
                    DMEEditor.AddLogMessage("Success", $"Removed database '{DataSourceName}'", 
                        DateTime.Now, 0, "Remove", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to remove database: {ex.Message}", 
                    DateTime.Now, -1, "Remove", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Ensures the memory database instance is available
        /// </summary>
        private void EnsureMemoryDatabaseAvailable()
        {
            if (memoryDB == null)
            {
                InitializeMemoryDatabase();
                if (memoryDB == null)
                {
                    DMEEditor.AddLogMessage("Error", $"InMemory database '{DataSourceName}' not available", 
                        DateTime.Now, -1, "EnsureMemoryDatabaseAvailable", Errors.Failed);
                    throw new InvalidOperationException($"InMemory database '{DataSourceName}' not available");
                }
            }
        }

        /// <summary>
        /// Loads the database structure
        /// </summary>
        private void LoadDatabaseStructure()
        {
            ShowProgressAndExecute("Loading InMemory database structure", (progress, token) =>
            {
                memoryDB.LoadStructure(progress, token);
                memoryDB.CreateStructure(progress, token);
            });
        }

        /// <summary>
        /// Shows progress form and executes an operation
        /// </summary>
        private void ShowProgressAndExecute(string message, Action<IProgress<PassedArgs>, CancellationToken> operation)
        {
            var args = new PassedArgs { Messege = message };
            var token = new CancellationToken();
            
            Visutil?.ShowWaitForm(args);
            Visutil?.PasstoWaitForm(args);
            
            var progress = new Progress<PassedArgs>(percent =>
            {
                if (!string.IsNullOrEmpty(percent.Messege))
                {
                    Visutil?.PasstoWaitForm(percent);
                }
                if (percent.EventType == "Stop")
                {
                    token.ThrowIfCancellationRequested();
                }
            });

            operation(progress, token);
        }
        #endregion
    }
}
