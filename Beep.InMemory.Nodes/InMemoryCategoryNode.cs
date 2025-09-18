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
    /// Category node for organizing in-memory databases by type or purpose
    /// Provides hierarchical organization and management of in-memory database connections
    /// </summary>
    [AddinAttribute(Caption = "InMemory Category", Name = "InMemoryCategoryNode.Beep", misc = "Beep", 
                    iconimage = "inmemorycategory.png", menu = "Beep", ObjectType = "Beep")]
    [AddinVisSchema(BranchType = EnumPointType.Category, BranchClass = "INMEMORY")]
    public class InMemoryCategoryNode : IBranch, IOrder
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
        public string BranchText { get; set; } = "InMemory Category";
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Category;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "inmemorycategory.png";
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
        public InMemoryCategoryNode()
        {
            InitializeNode();
        }

        public InMemoryCategoryNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, 
                                  string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
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
                BranchID = ID;
            }

            InitializeNode();
        }

        private void InitializeNode()
        {
            BranchClass = "INMEMORY";
            IconImageName = "inmemorycategory.png";
            BranchType = EnumPointType.Category;
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
                // Check if database node already exists
                if (ChildBranchs.Any(b => b.BranchText.Equals(connection.ConnectionName, 
                    StringComparison.InvariantCultureIgnoreCase)))
                {
                    DMEEditor.AddLogMessage("Warning", $"Database '{connection.ConnectionName}' already exists in category", 
                        DateTime.Now, 0, "CreateDBNode", Errors.Ok);
                    return DMEEditor.ErrorObject;
                }

                var database = new InMemoryDatabaseNode(TreeEditor, DMEEditor, this, 
                    connection.ConnectionName, TreeEditor.SeqID, EnumPointType.DataPoint, 
                    connection.ConnectionName);
                
                database.ConnectionProperties = connection;
                TreeEditor.Treebranchhandler.AddBranch(this, database);

                DMEEditor.AddLogMessage("Success", $"Added database '{connection.ConnectionName}' to category '{BranchText}'", 
                    DateTime.Now, 0, "CreateDBNode", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create database node: {ex.Message}", 
                    DateTime.Now, -1, "CreateDBNode", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Creates a sub-category node
        /// </summary>
        public IBranch CreateCategoryNode(CategoryFolder categoryFolder)
        {
            if (categoryFolder == null)
            {
                DMEEditor.AddLogMessage("Error", "Category folder cannot be null", 
                    DateTime.Now, -1, "CreateCategoryNode", Errors.Failed);
                return null;
            }

            try
            {
                // Check if sub-category already exists
                if (ChildBranchs.Any(b => b.BranchText.Equals(categoryFolder.FolderName, 
                    StringComparison.InvariantCultureIgnoreCase)))
                {
                    DMEEditor.AddLogMessage("Warning", $"Sub-category '{categoryFolder.FolderName}' already exists", 
                        DateTime.Now, 0, "CreateCategoryNode", Errors.Ok);
                    return ChildBranchs.First(b => b.BranchText.Equals(categoryFolder.FolderName, 
                        StringComparison.InvariantCultureIgnoreCase));
                }

                var subCategory = new InMemoryCategoryNode(TreeEditor, DMEEditor, this, 
                    categoryFolder.FolderName, TreeEditor.SeqID, EnumPointType.Category, 
                    "inmemorycategory.png");
                
                TreeEditor.Treebranchhandler.AddBranch(this, subCategory);
                subCategory.CreateChildNodes();

                DMEEditor.AddLogMessage("Success", $"Created sub-category '{categoryFolder.FolderName}' in '{BranchText}'", 
                    DateTime.Now, 0, "CreateCategoryNode", Errors.Ok);
                
                return subCategory;
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create sub-category: {ex.Message}", 
                    DateTime.Now, -1, "CreateCategoryNode", Errors.Failed);
                return null;
            }
        }

        /// <summary>
        /// Creates child nodes for databases and sub-categories in this category
        /// </summary>
        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                CreateDatabaseNodes();
                CreateSubCategoryNodes();

                DMEEditor.AddLogMessage("Success", $"Created child nodes for category '{BranchText}'", 
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
                    case "ADDDATABASE":
                        return AddDatabase();
                    case "ADDSUBCATEGORY":
                        return AddSubCategory();
                    case "REFRESH":
                        return RefreshCategory();
                    case "REMOVE":
                        return remove();
                    case "REMOVEALL":
                        return RemoveAllDatabases();
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

                DMEEditor.AddLogMessage("Success", $"Removed all child nodes from category '{BranchText}'", 
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

                DMEEditor.AddLogMessage("Success", $"InMemory category configuration set: {BranchText}", 
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
        /// Adds a new in-memory database to this category
        /// </summary>
        [CommandAttribute(Caption = "Add InMemory Database", Hidden = false, iconimage = "add.png")]
        public IErrorsInfo AddDatabase()
        {
            try
            {
                // This would typically open a dialog to create a new in-memory database
                // For now, we'll delegate to the root node's add functionality
                var rootNode = GetRootInMemoryNode();
                if (rootNode != null)
                {
                    var result = rootNode.ExecuteBranchAction("ADD");
                    if (result.Flag == Errors.Ok)
                    {
                        // Refresh this category to show the new database if it belongs here
                        RefreshCategory();
                    }
                    return result;
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", "Could not find InMemory root node", 
                        DateTime.Now, 0, "AddDatabase", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to add database: {ex.Message}", 
                    DateTime.Now, -1, "AddDatabase", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Adds a new sub-category
        /// </summary>
        [CommandAttribute(Caption = "Add Sub-Category", Hidden = false, iconimage = "createfolder.png")]
        public IErrorsInfo AddSubCategory()
        {
            try
            {
                var subCategoryName = $"SubCategory_{DateTime.Now:HHmmss}";
                
                var categoryFolder = new CategoryFolder
                {
                    FolderName = subCategoryName,
                    RootName = "INMEMORY",
                    ParentName = BranchText,
                    IsParentRoot = false,
                    IsParentFolder = true
                  
                };

                DMEEditor.ConfigEditor.CategoryFolders.Add(categoryFolder);
                CreateCategoryNode(categoryFolder);

                DMEEditor.AddLogMessage("Success", $"Created sub-category '{subCategoryName}' in '{BranchText}'", 
                    DateTime.Now, 0, "AddSubCategory", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to add sub-category: {ex.Message}", 
                    DateTime.Now, -1, "AddSubCategory", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Refreshes the category and its contents
        /// </summary>
        [CommandAttribute(Caption = "Refresh Category", Hidden = false, iconimage = "refresh.png")]
        public IErrorsInfo RefreshCategory()
        {
            try
            {
                RemoveChildNodes();
                CreateChildNodes();

                DMEEditor.AddLogMessage("Success", $"Refreshed category '{BranchText}'", 
                    DateTime.Now, 0, "RefreshCategory", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to refresh category: {ex.Message}", 
                    DateTime.Now, -1, "RefreshCategory", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Removes all databases from this category
        /// </summary>
        [CommandAttribute(Caption = "Remove All Databases", Hidden = false, iconimage = "remove.png")]
        public IErrorsInfo RemoveAllDatabases()
        {
            try
            {
                var result = Visutil?.DialogManager?.InputBoxYesNo("Confirm Removal", 
                    $"Are you sure you want to remove all databases from category '{BranchText}'?");
                
                if (result == BeepDialogResult.Yes)
                {
                    var databasesToRemove = ChildBranchs
                        .Where(b => b.BranchType == EnumPointType.DataPoint)
                        .ToArray();
                    
                    foreach (var database in databasesToRemove)
                    {
                        TreeEditor.Treebranchhandler.RemoveBranch(database);
                    }

                    DMEEditor.AddLogMessage("Success", $"Removed all databases from category '{BranchText}'", 
                        DateTime.Now, 0, "RemoveAllDatabases", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to remove databases: {ex.Message}", 
                    DateTime.Now, -1, "RemoveAllDatabases", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Removes this category
        /// </summary>
        [CommandAttribute(Caption = "Remove Category", Hidden = false, iconimage = "remove.png")]
        public IErrorsInfo remove()
        {
            try
            {
                var result = Visutil?.DialogManager?.InputBoxYesNo("Confirm Removal", 
                    $"Are you sure you want to remove category '{BranchText}'? This will remove all its contents.");
                
                if (result == BeepDialogResult.Yes)
                {
                    RemoveChildNodes();
                    TreeEditor?.Treebranchhandler?.RemoveBranch(this);

                    DMEEditor.AddLogMessage("Success", $"Removed category '{BranchText}'", 
                        DateTime.Now, 0, "Remove", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to remove category: {ex.Message}", 
                    DateTime.Now, -1, "Remove", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Creates database nodes for connections assigned to this category
        /// </summary>
        private void CreateDatabaseNodes()
        {
            var categoryFolders = DMEEditor.ConfigEditor.CategoryFolders
                .Where(x => x.RootName.Equals("INMEMORY", StringComparison.InvariantCultureIgnoreCase) && 
                           x.FolderName.Equals(BranchText, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            foreach (var folder in categoryFolders)
            {
                if (folder.items != null)
                {
                    foreach (var item in folder.items)
                    {
                        var connection = DMEEditor.ConfigEditor.DataConnections
                            .FirstOrDefault(x => x.ConnectionName.Equals(item, StringComparison.InvariantCultureIgnoreCase));

                        if (connection != null && !connection.Drawn)
                        {
                            CreateDBNode(connection);
                            connection.Drawn = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates nodes for sub-categories
        /// </summary>
        private void CreateSubCategoryNodes()
        {
            var subCategories = DMEEditor.ConfigEditor.CategoryFolders
                .Where(x => x.RootName.Equals("INMEMORY", StringComparison.InvariantCultureIgnoreCase) && 
                           x.ParentName.Equals(BranchText, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            foreach (var subCategory in subCategories)
            {
                if (!ChildBranchs.Any(b => b.BranchText.Equals(subCategory.FolderName, 
                    StringComparison.InvariantCultureIgnoreCase) && b.BranchType == EnumPointType.Category))
                {
                    CreateCategoryNode(subCategory);
                }
            }
        }

        /// <summary>
        /// Gets the root InMemory node for delegation
        /// </summary>
        private IBranch GetRootInMemoryNode()
        {
            return TreeEditor?.Branches?.FirstOrDefault(b => 
                b.BranchType == EnumPointType.Root && 
                b.BranchClass.Equals("INMEMORY", StringComparison.InvariantCultureIgnoreCase));
        }
        #endregion
    }
}
