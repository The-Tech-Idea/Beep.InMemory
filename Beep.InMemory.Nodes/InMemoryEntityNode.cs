using System.Data;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;

namespace Beep.InMemory.Nodes
{
    /// <summary>
    /// Represents an entity (table/view) within an in-memory database
    /// Provides operations for data manipulation, structure analysis, and entity management
    /// </summary>
    [AddinAttribute(Caption = "InMemory Entity", Name = "InMemoryEntityNode.Beep", misc = "Beep", 
                    iconimage = "inmemoryentity.png", menu = "Beep", ObjectType = "Beep")]
    public class InMemoryEntityNode : IBranch
    {
        #region Properties
        public bool Visible { get; set; } = true;
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public IBranch ParentBranch { get; set; }
        public int ID { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public int Order { get; set; }
        public string Name { get; set; } = "";
        public string BranchText { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Entity;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "inmemoryentity.png";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "INMEMORY";
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; } = new List<string>();
        public object TreeStrucure { get; set; }
        public IAppManager Visutil { get; set; }
        public string ObjectType { get; set; } = "Beep";
        public int MiscID { get; set; }
        public bool IsDataSourceNode { get; set; } = false;
        public string MenuID { get; set; }

        /// <summary>
        /// Tracks if the entity is currently being processed
        /// </summary>
        private bool _isProcessing = false;
        #endregion

        #region Constructors
        public InMemoryEntityNode()
        {
            InitializeNode();
        }

        public InMemoryEntityNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, 
                                 string pBranchText, int pID, EnumPointType pBranchType, 
                                 string pimagename, IDataSource ds)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranch = pParentNode;
            ParentBranchID = pParentNode?.ID ?? 0;
            BranchText = pBranchText;
            BranchType = EnumPointType.Entity;
            DataSource = ds;
            DataSourceName = ds?.DatasourceName;
            
            if (pID != 0)
            {
                ID = pID;
                BranchID = ID;
            }

            InitializeNode();
            InitializeEntityStructure();
        }

        private void InitializeNode()
        {
            BranchClass = "INMEMORY";
            IconImageName = "inmemoryentity.png";
            BranchType = EnumPointType.Entity;
            ObjectType = "Beep";
            ChildBranchs = new List<IBranch>();
            BranchActions = new List<string>();
        }

        private void InitializeEntityStructure()
        {
            try
            {
                if (DataSource != null && !string.IsNullOrEmpty(BranchText))
                {
                    EntityStructure = new EntityStructure
                    {
                        DataSourceID = DataSource.DatasourceName,
                        Viewtype = ViewType.Table,
                        EntityName = BranchText,
                        DatasourceEntityName = BranchText
                    };
                }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage("Warning", $"Failed to initialize entity structure: {ex.Message}", 
                    DateTime.Now, 0, "InitializeEntityStructure", Errors.Ok);
            }
        }
        #endregion

        #region Interface Methods
        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                // In-memory entities typically don't have child nodes
                // Could be extended to show field/column information
                DMEEditor.AddLogMessage("Success", $"Entity '{BranchText}' ready", 
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
                    case "DATAEDIT":
                        return DataEdit();
                    case "VIEWSTRUCTURE":
                        return ViewStructure();
                    case "FIELDPROPERTIES":
                        return FieldProperties();
                    case "DROPENTITY":
                        return DropEntity();
                    case "CREATEVIEW":
                        return CreateView();
                    case "EXPORTDATA":
                        return ExportEntityData();
                    case "VIEWDATA":
                        return ViewEntityData();
                    case "REFRESHENTITY":
                        return RefreshEntity();
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

                DMEEditor.AddLogMessage("Success", $"Removed child nodes from entity '{BranchText}'", 
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
                IconImageName = pimagename;
                
                if (pID != 0)
                {
                    ID = pID;
                    BranchID = pID;
                }

                DMEEditor.AddLogMessage("Success", $"InMemory entity configuration set: {BranchText}", 
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
            // Entities don't create categories
            return null;
        }
        #endregion

        #region Public Commands
        /// <summary>
        /// Opens the data editor for this entity
        /// </summary>
        [CommandAttribute(Caption = "Data Edit", iconimage = "edit_entity.png")]
        public IErrorsInfo DataEdit()
        {
            if (_isProcessing) return DMEEditor.ErrorObject;

            try
            {
                _isProcessing = true;
                ValidateEntityAndDataSource();

                var objectItems = new List<ObjectItem>
                {
                    new ObjectItem { obj = this, Name = "Branch" }
                };

                var passedArgs = new PassedArgs
                {
                    CurrentEntity = BranchText,
                    Id = BranchID,
                    ObjectType = "INMEMORYENTITY",
                    DataSource = DataSource,
                    ObjectName = BranchText,
                    Objects = objectItems,
                    DatasourceName = DataSource.DatasourceName,
                    EventType = "CRUDENTITY"
                };

                Visutil.ShowPage("uc_crudView", passedArgs);
                DMEEditor.AddLogMessage("Success", $"Opened data editor for entity '{BranchText}'", 
                    DateTime.Now, 0, "DataEdit", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to open data editor: {ex.Message}", 
                    DateTime.Now, -1, "DataEdit", Errors.Failed);
            }
            finally
            {
                _isProcessing = false;
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Views the entity data in read-only mode
        /// </summary>
        [CommandAttribute(Caption = "View Data", iconimage = "view.png")]
        public IErrorsInfo ViewEntityData()
        {
            if (_isProcessing) return DMEEditor.ErrorObject;

            try
            {
                _isProcessing = true;
                ValidateEntityAndDataSource();

                if (DataSource != null)
                {
                    var data = DataSource.GetEntity(BranchText, null);
                    if (data != null)
                    {
                        int rowCount = GetRowCount(data);
                        DMEEditor.AddLogMessage("Success", $"Entity '{BranchText}' contains {rowCount} rows", 
                            DateTime.Now, 0, "ViewEntityData", Errors.Ok);
                        
                        // Here you could open a data viewer or display the data
                        // Implementation depends on available UI components
                    }
                    else
                    {
                        DMEEditor.AddLogMessage("Warning", $"No data found in entity '{BranchText}'", 
                            DateTime.Now, 0, "ViewEntityData", Errors.Ok);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to view entity data: {ex.Message}", 
                    DateTime.Now, -1, "ViewEntityData", Errors.Failed);
            }
            finally
            {
                _isProcessing = false;
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Creates a view based on this entity
        /// </summary>
        [CommandAttribute(Caption = "Create View", iconimage = "createentity.png")]
        public IErrorsInfo CreateView()
        {
            try
            {
                ValidateEntityAndDataSource();

                var passedArgs = new PassedArgs
                {
                    ObjectName = "DATABASE",
                    ObjectType = "TABLE",
                    EventType = "CREATEVIEWBASEDONTABLE",
                    ParameterString1 = "Create View using Table",
                    Objects = new List<ObjectItem> { new ObjectItem { Name = "Branch", obj = this } }
                };

                DMEEditor.Passedarguments = passedArgs;
                
                var viewBranch = TreeEditor.Branches
                    .FirstOrDefault(x => x.BranchType == EnumPointType.Root && x.BranchClass == "VIEW");
                
                if (viewBranch != null)
                {
                    TreeEditor.Treebranchhandler.SendActionFromBranchToBranch(viewBranch, this, "Create View using Table");
                    DMEEditor.AddLogMessage("Success", $"Initiated view creation for entity '{BranchText}'", 
                        DateTime.Now, 0, "CreateView", Errors.Ok);
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", "View root branch not found", 
                        DateTime.Now, 0, "CreateView", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to create view: {ex.Message}", 
                    DateTime.Now, -1, "CreateView", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Views the entity structure and schema information
        /// </summary>
        [CommandAttribute(Caption = "View Structure", Hidden = false, iconimage = "structure.png")]
        public IErrorsInfo ViewStructure()
        {
            try
            {
                ValidateEntityAndDataSource();

                var objectItems = new List<ObjectItem>
                {
                    new ObjectItem { obj = this, Name = "Branch" },
                    new ObjectItem { Name = "TitleText", obj = $"Structure of {BranchText}" }
                };

                var passedArgs = new PassedArgs
                {
                    CurrentEntity = BranchText,
                    Id = BranchID,
                    ObjectType = "INMEMORYENTITY",
                    DataSource = DataSource,
                    ObjectName = EntityStructure.DataSourceID,
                    Objects = objectItems,
                    DatasourceName = EntityStructure.DataSourceID,
                    EventType = "INMEMORYENTITY"
                };

                Visutil.ShowPage("uc_DataEntityStructureViewer", passedArgs);
                DMEEditor.AddLogMessage("Success", $"Opened structure viewer for entity '{BranchText}'", 
                    DateTime.Now, 0, "ViewStructure", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to view structure: {ex.Message}", 
                    DateTime.Now, -1, "ViewStructure", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Opens field properties editor for the entity
        /// </summary>
        [CommandAttribute(Caption = "Field Properties", iconimage = "properties.png")]
        public IErrorsInfo FieldProperties()
        {
            try
            {
                ValidateEntityAndDataSource();

                var objectItems = new List<ObjectItem>
                {
                    new ObjectItem { obj = this, Name = "Branch" },
                    new ObjectItem { Name = "TitleText", obj = $"Fields of {BranchText}" }
                };

                var passedArgs = new PassedArgs
                {
                    CurrentEntity = BranchText,
                    Id = 0,
                    ObjectType = "DEFAULTS",
                    ObjectName = DataSourceName,
                    Objects = objectItems,
                    DatasourceName = DataSourceName,
                    EventType = "DEFAULTS"
                };

                Visutil.ShowPage("uc_fieldproperty", passedArgs);
                DMEEditor.AddLogMessage("Success", $"Opened field properties for entity '{BranchText}'", 
                    DateTime.Now, 0, "FieldProperties", Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to open field properties: {ex.Message}", 
                    DateTime.Now, -1, "FieldProperties", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Exports entity data to external format
        /// </summary>
        [CommandAttribute(Caption = "Export Data", iconimage = "export.png")]
        public IErrorsInfo ExportEntityData()
        {
            try
            {
                ValidateEntityAndDataSource();

                var data = DataSource.GetEntity(BranchText, null);
                if (data != null)
                {
                    int rowCount = GetRowCount(data);
                    DMEEditor.AddLogMessage("Success", $"Export functionality available for entity '{BranchText}' ({rowCount} rows)", 
                        DateTime.Now, 0, "ExportEntityData", Errors.Ok);
                    
                    // Implementation for export functionality would go here
                    // This could open an export dialog or trigger an export process
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", $"No data to export from entity '{BranchText}'", 
                        DateTime.Now, 0, "ExportEntityData", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to export entity data: {ex.Message}", 
                    DateTime.Now, -1, "ExportEntityData", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Refreshes the entity structure and metadata
        /// </summary>
        [CommandAttribute(Caption = "Refresh Entity", iconimage = "refresh.png")]
        public IErrorsInfo RefreshEntity()
        {
            try
            {
                ValidateEntityAndDataSource();

                EntityStructure = DataSource.GetEntityStructure(BranchText, true);
                if (EntityStructure != null)
                {
                    DMEEditor.AddLogMessage("Success", $"Refreshed entity structure for '{BranchText}'", 
                        DateTime.Now, 0, "RefreshEntity", Errors.Ok);
                }
                else
                {
                    DMEEditor.AddLogMessage("Warning", $"Could not refresh entity structure for '{BranchText}'", 
                        DateTime.Now, 0, "RefreshEntity", Errors.Ok);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to refresh entity: {ex.Message}", 
                    DateTime.Now, -1, "RefreshEntity", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Drops (deletes) the entity from the database
        /// </summary>
        [CommandAttribute(Caption = "Drop Entity", iconimage = "remove.png")]
        public IErrorsInfo DropEntity()
        {
            try
            {
                ValidateEntityAndDataSource();

                var result = Visutil?.DialogManager?.InputBoxYesNo("Confirm Drop", 
                    $"Are you sure you want to drop entity '{BranchText}'?");
                
                if (result.Result == BeepDialogResult.Yes)
                {
                    RefreshEntityStructure();
                    
                    if (EntityStructure != null)
                    {
                        bool entityExists = CheckEntityExists();
                        
                        if (entityExists && DataSource.CheckEntityExist(EntityStructure.EntityName))
                        {
                            DataSource.ExecuteSql($"DROP TABLE {EntityStructure.DatasourceEntityName}");
                        }

                        if (DMEEditor.ErrorObject.Flag == Errors.Ok || !entityExists)
                        {
                            RemoveEntityFromDataSource();
                            TreeEditor.Treebranchhandler.RemoveBranch(this);
                            
                            DMEEditor.AddLogMessage("Success", $"Dropped entity '{EntityStructure.EntityName}'", 
                                DateTime.Now, 0, "DropEntity", Errors.Ok);
                        }
                        else
                        {
                            DMEEditor.AddLogMessage("Error", $"Failed to drop entity '{EntityStructure.EntityName}' - {DMEEditor.ErrorObject.Message}", 
                                DateTime.Now, -1, "DropEntity", Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Failed to drop entity: {ex.Message}", 
                    DateTime.Now, -1, "DropEntity", Errors.Failed);
            }

            return DMEEditor.ErrorObject;
        }
        #endregion

        #region Private Helper Methods
        /// <summary>
        /// Validates that entity and data source are available
        /// </summary>
        private void ValidateEntityAndDataSource()
        {
            if (string.IsNullOrEmpty(BranchText))
                throw new InvalidOperationException("Entity name is not set");

            if (DataSource == null)
                throw new InvalidOperationException("Data source is not available");
        }

        /// <summary>
        /// Gets the row count from data object
        /// </summary>
        private int GetRowCount(object data)
        {
            return data switch
            {
                DataTable dt => dt.Rows.Count,
                System.Collections.ICollection collection => collection.Count,
                _ => 0
            };
        }

        /// <summary>
        /// Refreshes the entity structure from the data source
        /// </summary>
        private void RefreshEntityStructure()
        {
            EntityStructure = DataSource.GetEntityStructure(BranchText, true);
        }

        /// <summary>
        /// Checks if the entity exists in the data source
        /// </summary>
        private bool CheckEntityExists()
        {
            try
            {
                var entityIndex = DataSource.GetEntityIdx(EntityStructure.EntityName);
                return entityIndex >= 0 && DataSource.Entities[entityIndex].IsCreated;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the entity from the data source entities list
        /// </summary>
        private void RemoveEntityFromDataSource()
        {
            try
            {
                var entityIndex = DataSource.Entities.FindIndex(p => 
                    p.DatasourceEntityName == EntityStructure.DatasourceEntityName);
                
                if (entityIndex >= 0)
                {
                    DataSource.Entities.RemoveAt(entityIndex);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Warning", $"Failed to remove entity from data source list: {ex.Message}", 
                    DateTime.Now, 0, "RemoveEntityFromDataSource", Errors.Ok);
            }
        }
        #endregion
    }
}
