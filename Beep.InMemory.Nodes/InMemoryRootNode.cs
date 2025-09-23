using Beep.InMemory.Logic;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using System.Reflection;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.DriversConfigurations;
using static TheTechIdea.Beep.Utils.Util;

namespace Beep.InMemory.Nodes
{
    /// <summary>
    /// Root node for managing in-memory database connections and categories
    /// Provides centralized management of all in-memory database operations
    /// </summary>
    [AddinAttribute(Caption = "InMemory", Name = "InMemoryNode.Beep", misc = "Beep", 
                    iconimage = "inmemoryroot.png", menu = "DataSource", ObjectType = "Beep", 
                    Category = DatasourceCategory.INMEMORY)]
    [AddinVisSchema(BranchType = EnumPointType.Root, BranchClass = "DATASOURCEROOT", 
                    RootNodeName = "DataSourcesRootNode")]
    public class InMemoryRootNode : IBranch, IOrder
    {
        #region Properties
        public bool Visible { get; set; } = true;
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public IBranch ParentBranch { get; set; }
        public string ObjectType { get; set; } = "Beep";
        public int ID { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public int Order { get; set; } = 4;
        public string Name { get; set; }
        public string BranchText { get; set; } = "InMemory";
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Root;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "inmemoryroot.png";
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
        public bool IsDataSourceNode { get; set; } = false;
        public string MenuID { get; set; }
        #endregion

        #region Constructors
        public InMemoryRootNode()
        {
            InitializeNode();
        }

        public InMemoryRootNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, 
                              string pBranchText, int pID, EnumPointType pBranchType, 
                              string pimagename, string ConnectionName)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            InitializeNode();
        }

        private void InitializeNode()
        {
            BranchText = "InMemory";
            BranchClass = "INMEMORY";
            IconImageName = "inmemoryroot.png";
            BranchType = EnumPointType.Root;
            ObjectType = "Beep";
            Order = 4;
            ChildBranchs = new List<IBranch>();
            BranchActions = new List<string>();
        }
        #endregion

        #region Interface Methods
        /// <summary>
        /// Creates a database node for an in-memory database connection
        /// </summary>
        public IErrorsInfo CreateDBNode(ConnectionProperties connection)
        {
            if (connection == null)
            {
                DMEEditor.AddLogMessage("Error", "Connection properties cannot be null", 
                    DateTime.Now, -1, "CreateDBNode", Errors.Failed);
                return DMEEditor.ErrorObject;
            }

            try
            {
                // Check if node already exists
                if (ChildBranchs.Any(b => b.BranchText.Equals(connection.ConnectionName, 
                    StringComparison.InvariantCultureIgnoreCase)))
                {
                    DMEEditor.AddLogMessage("Warning", $"Database node '{connection.ConnectionName}' already exists", 
                        DateTime.Now, 0, "CreateDBNode", Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                var driversConfig = GetDriverConfiguration(connection);
                if (driversConfig == null)
                {
                    DMEEditor.AddLogMessage("Error", $"No driver configuration found for connection '{connection.ConnectionName}'", 
                        DateTime.Now, -1, "CreateDBNode", Errors.Failed);
                    return DMEEditor.ErrorObject;
                }

                var databaseNode = CreateDatabaseNodeInstance(connection, driversConfig);
                if (databaseNode != null)
                {
                    TreeEditor.Treebranchhandler.AddBranch(this, databaseNode);
                    DMEEditor.AddLogMessage("Success", $"Created database node '{connection.ConnectionName}'", 
                        DateTime.Now, 0, "CreateDBNode", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create database node: {ex.Message}", 
                    DateTime.Now, -1, "CreateDBNode", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Creates a category node for organizing in-memory databases
        /// </summary>
        public IBranch CreateCategoryNode(CategoryFolder categoryFolder)
        {
            if (categoryFolder == null)
            {
                DMEEditor.AddLogMessage("Error", "Category folder cannot be null", 
                    DateTime.Now, -1, "CreateCategoryNode", Errors.Failed);
                return null;
            }

            InMemoryCategoryNode category = null;
            try
            {
                // Check if category already exists
                if (ChildBranchs.Any(b => b.BranchText.Equals(categoryFolder.FolderName, 
                    StringComparison.InvariantCultureIgnoreCase)))
                {
                    DMEEditor.AddLogMessage("Warning", $"Category '{categoryFolder.FolderName}' already exists", 
                        DateTime.Now, 0, "CreateCategoryNode", Errors.Ok);
                    return ChildBranchs.First(b => b.BranchText.Equals(categoryFolder.FolderName, 
                        StringComparison.InvariantCultureIgnoreCase));
                }

                category = new InMemoryCategoryNode(TreeEditor, DMEEditor, this, 
                    categoryFolder.FolderName, TreeEditor.SeqID, EnumPointType.Category, 
                    TreeEditor.CategoryIcon);
                
                TreeEditor.Treebranchhandler.AddBranch(this, category);
                category.CreateChildNodes();

                DMEEditor.AddLogMessage("Success", $"Created category node '{categoryFolder.FolderName}'", 
                    DateTime.Now, 0, "CreateCategoryNode", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create category node: {ex.Message}", 
                    DateTime.Now, -1, "CreateCategoryNode", Errors.Failed);
            }

            return category;
        }

        /// <summary>
        /// Creates child nodes for all in-memory databases and categories
        /// </summary>
        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                CreateInMemoryDatabaseNodes();
                CreateCategoryNodes();
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
                    case "REFRESH":
                        return RefreshInMemoryNodes();
                    case "ADD":
                        return Add();
                    case "REMOVE":
                        return remove();
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

                DMEEditor.AddLogMessage("Success", "Removed all child nodes", 
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
                
                if (pID != 0)
                {
                    ID = pID;
                    BranchID = pID;
                }

                DMEEditor.AddLogMessage("Success", "InMemory root node configuration set", 
                    DateTime.Now, 0, "SetConfig", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to set configuration: {ex.Message}", 
                    DateTime.Now, -1, "SetConfig", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }
        #endregion

        #region Public Commands
        /// <summary>
        /// Adds a new in-memory database
        /// </summary>
        [CommandAttribute(Caption = "Add InMemory Database", Hidden = false, iconimage = "add.png")]
        public IErrorsInfo Add()
        {
            try
            {
                var connection = BeepInMemoryManager.CreateInMemoryDB(DMEEditor, Visutil);
                if (DMEEditor.ErrorObject.Flag == Errors.Ok && connection != null)
                {
                    CreateDBNode(connection);
                    DMEEditor.AddLogMessage("Success", $"Added in-memory database '{connection.ConnectionName}'", 
                        DateTime.Now, 0, "Add", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to add in-memory database: {ex.Message}", 
                    DateTime.Now, -1, "Add", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Removes all in-memory databases (with confirmation)
        /// </summary>
        [CommandAttribute(Caption = "Remove All", Hidden = false, iconimage = "remove.ico")]
        public IErrorsInfo remove()
        {
            try
            {
                if (Visutil?.DialogManager?.InputBoxYesNo("Confirm Removal", 
                    "Are you sure you want to remove all in-memory databases?").Result == BeepDialogResult.Yes)
                {
                    RemoveChildNodes();
                    DMEEditor.AddLogMessage("Success", "Removed all in-memory databases", 
                        DateTime.Now, 0, "Remove", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to remove databases: {ex.Message}", 
                    DateTime.Now, -1, "Remove", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Refreshes all in-memory database nodes
        /// </summary>
        [CommandAttribute(Caption = "Refresh", Hidden = false, iconimage = "refresh.png")]
        public IErrorsInfo RefreshInMemoryNodes()
        {
            try
            {
                RemoveChildNodes();
                CreateChildNodes();
                DMEEditor.AddLogMessage("Success", "Refreshed in-memory database nodes", 
                    DateTime.Now, 0, "Refresh", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to refresh nodes: {ex.Message}", 
                    DateTime.Now, -1, "Refresh", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Gets driver configuration for a connection
        /// </summary>
        private ConnectionDriversConfig GetDriverConfiguration(ConnectionProperties connection)
        {
            try
            {
                return DMEEditor.Utilfunction.LinkConnection2Drivers(connection);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to get driver configuration: {ex.Message}", 
                    DateTime.Now, -1, "GetDriverConfiguration", Errors.Failed);
                return null;
            }
        }

        /// <summary>
        /// Creates a database node instance using reflection or default implementation
        /// </summary>
        private IBranch CreateDatabaseNodeInstance(ConnectionProperties connection, ConnectionDriversConfig driversConfig)
        {
            try
            {
                // Try to create using reflection first
                var reflectionNode = CreateNodeUsingReflection(connection, driversConfig);
                if (reflectionNode != null)
                    return reflectionNode;

                // Fallback to default implementation
                return CreateDefaultDatabaseNode(connection, driversConfig);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create database node instance: {ex.Message}", 
                    DateTime.Now, -1, "CreateDatabaseNodeInstance", Errors.Failed);
                return null;
            }
        }

        /// <summary>
        /// Creates a database node using reflection
        /// </summary>
        private IBranch CreateNodeUsingReflection(ConnectionProperties connection, ConnectionDriversConfig driversConfig)
        {
            try
            {
                var classDefinition = DMEEditor.ConfigEditor.BranchesClasses?
                    .FirstOrDefault(p => p.classProperties?.ClassType?.StartsWith(driversConfig.classHandler) == true);

                if (classDefinition == null)
                    return null;

                var nodeType = DMEEditor.assemblyHandler.GetType(classDefinition.PackageName);
                if (nodeType == null)
                    return null;

                var constructor = nodeType.GetConstructors()
                    .FirstOrDefault(o => o.GetParameters().Length == 0);
                
                if (constructor == null)
                    return null;

                var activator = GetActivator<IBranch>(constructor);
                var branch = activator();

                ConfigureBranchInstance(branch, connection, driversConfig);
                return branch;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Warning", $"Reflection-based node creation failed: {ex.Message}", 
                    DateTime.Now, 0, "CreateNodeUsingReflection", Errors.Ok);
                return null;
            }
        }

        /// <summary>
        /// Creates a default database node
        /// </summary>
        private IBranch CreateDefaultDatabaseNode(ConnectionProperties connection, ConnectionDriversConfig driversConfig)
        {
            var database = new InMemoryDatabaseNode(TreeEditor, DMEEditor, this, 
                connection.ConnectionName, TreeEditor.SeqID, EnumPointType.DataPoint, 
                connection.ConnectionName);
            
            ConfigureBranchInstance(database, connection, driversConfig);
            return database;
        }

        /// <summary>
        /// Configures a branch instance with connection properties
        /// </summary>
        private void ConfigureBranchInstance(IBranch branch, ConnectionProperties connection, ConnectionDriversConfig driversConfig)
        {
            try
            {
                // Set connection properties using reflection
                var connectionProp = branch.GetType().GetProperty("ConnectionProperties");
                connectionProp?.SetValue(branch, connection);

                // Set basic properties
                branch.DMEEditor = DMEEditor;
                branch.ID = TreeEditor.SeqID;
                branch.BranchID = TreeEditor.SeqID;
                branch.ParentBranchID = ID;
                branch.BranchText = connection.ConnectionName;
                branch.DataSourceName = connection.ConnectionName;
                branch.TreeEditor = TreeEditor;
                branch.IconImageName = driversConfig.iconname ?? "inmemorydatabase.png";

                // Set data source
                var memoryDB = DMEEditor.GetDataSource(connection.ConnectionName) as IInMemoryDB;
                branch.DataSource = memoryDB as IDataSource;

                // Set memory DB using reflection
                var memoryDBProp = branch.GetType().GetProperty("memoryDB");
                memoryDBProp?.SetValue(branch, memoryDB);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Warning", $"Failed to configure branch instance: {ex.Message}", 
                    DateTime.Now, 0, "ConfigureBranchInstance", Errors.Ok);
            }
        }

        /// <summary>
        /// Creates nodes for all in-memory database connections
        /// </summary>
        private void CreateInMemoryDatabaseNodes()
        {
            var inMemoryConnections = DMEEditor.ConfigEditor.DataConnections
                .Where(c => c.IsInMemory == true && c.Category == DatasourceCategory.INMEMORY)
                .ToList();

            foreach (var connection in inMemoryConnections)
            {
                if (TreeEditor.Treebranchhandler.CheckifBranchExistinCategory(connection.ConnectionName, "INMEMORY") == null &&
                    !ChildBranchs.Any(p => p.BranchText.Equals(connection.ConnectionName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    CreateDBNode(connection);
                    connection.Drawn = true;
                }
            }
        }

        /// <summary>
        /// Creates category nodes for organization
        /// </summary>
        private void CreateCategoryNodes()
        {
            var categoryFolders = DMEEditor.ConfigEditor.CategoryFolders
                .Where(x => x.RootName.Equals("INMEMORY", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            foreach (var folder in categoryFolders)
            {
                if (!ChildBranchs.Any(p => p.BranchText == folder.FolderName && 
                    p.BranchClass.Contains("INMEMORY")))
                {
                    CreateCategoryNode(folder);
                }
            }
        }
        #endregion
    }
}
