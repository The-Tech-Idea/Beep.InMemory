using Beep.InMemory.Nodes;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Logger;

namespace TheTechIdea.Beep
{
    /// <summary>
    /// Provides default methods for data source operations in tree nodes
    /// Contains shared functionality for entity management and structure operations
    /// </summary>
    public static class DataSourceDefaultMethods
    {
        /// <summary>
        /// Refreshes entities by clearing existing ones and reloading from the data source
        /// </summary>
        /// <param name="DatabaseBranch">The database branch to refresh</param>
        /// <param name="DMEEditor">The DME editor instance</param>
        /// <param name="Visutil">The visual utility manager</param>
        /// <returns>Error information</returns>
        public static async Task<IErrorsInfo> RefreshEntitiesAsync(IBranch DatabaseBranch, IDMEEditor DMEEditor, IAppManager Visutil)
        {
            if (DatabaseBranch == null)
            {
                DMEEditor.AddLogMessage("Error", "Database branch cannot be null", 
                    DateTime.Now, -1, "RefreshEntities", Errors.Failed);
                return DMEEditor.ErrorObject;
            }

            var tree = Visutil?.Tree as ITree;
            if (tree == null)
            {
                DMEEditor.AddLogMessage("Error", "Tree interface not available", 
                    DateTime.Now, -1, "RefreshEntities", Errors.Failed);
                return DMEEditor.ErrorObject;
            }

            string branchText = DatabaseBranch.BranchText;
            string dataSourceName = DatabaseBranch.DataSourceName;
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            
            var passedArgs = new PassedArgs { DatasourceName = branchText };

            try
            {
                var dataSource = DMEEditor.GetDataSource(branchText);
                if (dataSource == null)
                {
                    DMEEditor.AddLogMessage("Error", $"Data source '{branchText}' not found", 
                        DateTime.Now, -1, "RefreshEntities", Errors.Failed);
                    return DMEEditor.ErrorObject;
                }

                Visutil.ShowWaitForm(passedArgs);

                // Ensure connection is open
                if (dataSource.ConnectionStatus != System.Data.ConnectionState.Open)
                {
                    dataSource.Openconnection();
                }

                if (dataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                {
                    var userConfirmed = Visutil.DialogManager?.InputBoxYesNo("Refresh Entities", 
                        "Are you sure? This might take some time and will reload all entities.");
                    
                    if (userConfirmed.Result == BeepDialogResult.Yes || userConfirmed.Result == BeepDialogResult.OK)
                    {
                        await ProcessEntityRefresh(DatabaseBranch, dataSource, tree, Visutil, passedArgs, dataSourceName);
                    }
                }
                else
                {
                    passedArgs.Messege = "Could not open connection to data source";
                    Visutil.PasstoWaitForm(passedArgs);
                    DMEEditor.AddLogMessage("Error", $"Failed to open connection to '{branchText}'", 
                        DateTime.Now, -1, "RefreshEntities", Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Error refreshing entities: {ex.Message}", 
                    DateTime.Now, -1, "RefreshEntities", Errors.Failed);
                passedArgs.Messege = "Error occurred during refresh";
                Visutil.PasstoWaitForm(passedArgs);
            }
            finally
            {
                await Visutil.CloseWaitFormAsync();
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Synchronous wrapper for RefreshEntitiesAsync
        /// </summary>
        public static IErrorsInfo RefreshEntities(IBranch DatabaseBranch, IDMEEditor DMEEditor, IAppManager Visutil)
        {
            return RefreshEntitiesAsync(DatabaseBranch, DMEEditor, Visutil).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets entities from the data source and creates tree nodes for them
        /// </summary>
        /// <param name="DatabaseBranch">The database branch to populate</param>
        /// <param name="DMEEditor">The DME editor instance</param>
        /// <param name="Visutil">The visual utility manager</param>
        /// <returns>Error information</returns>
        public static async Task<IErrorsInfo> GetEntitiesAsync(IBranch DatabaseBranch, IDMEEditor DMEEditor, IAppManager Visutil)
        {
            if (DatabaseBranch == null)
            {
                DMEEditor.AddLogMessage("Error", "Database branch cannot be null", 
                    DateTime.Now, -1, "GetEntities", Errors.Failed);
                return DMEEditor.ErrorObject;
            }

            var tree = Visutil?.Tree as ITree;
            if (tree == null)
            {
                DMEEditor.AddLogMessage("Error", "Tree interface not available", 
                    DateTime.Now, -1, "GetEntities", Errors.Failed);
                return DMEEditor.ErrorObject;
            }

            string branchText = DatabaseBranch.BranchText;
            string dataSourceName = DatabaseBranch.DataSourceName;
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            
            var passedArgs = new PassedArgs { DatasourceName = branchText };

            try
            {
                var dataSource = DMEEditor.GetDataSource(branchText);
                if (dataSource == null)
                {
                    DMEEditor.AddLogMessage("Error", $"Data source '{branchText}' not found", 
                        DateTime.Now, -1, "GetEntities", Errors.Failed);
                    return DMEEditor.ErrorObject;
                }

                 Visutil.ShowWaitForm(passedArgs);

                // Ensure connection is open
                dataSource.Openconnection();
                
                if (dataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                {
                    await ProcessEntityLoad(DatabaseBranch, dataSource, tree, Visutil, passedArgs, dataSourceName);
                }
                else
                {
                    passedArgs.Messege = "Could not open connection to data source";
                    Visutil.PasstoWaitForm(passedArgs);
                    DMEEditor.AddLogMessage("Error", $"Failed to open connection to '{branchText}'", 
                        DateTime.Now, -1, "GetEntities", Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", $"Error getting entities: {ex.Message}", 
                    DateTime.Now, -1, "GetEntities", Errors.Failed);
                passedArgs.Messege = "Error occurred while loading entities";
                Visutil.PasstoWaitForm(passedArgs);
            }
            finally
            {
                await Visutil.CloseWaitFormAsync();
            }

            return DMEEditor.ErrorObject;
        }

        /// <summary>
        /// Synchronous wrapper for GetEntitiesAsync
        /// </summary>
        public static IErrorsInfo GetEntities(IBranch DatabaseBranch, IDMEEditor DMEEditor, IAppManager Visutil)
        {
            return GetEntitiesAsync(DatabaseBranch, DMEEditor, Visutil).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates entity nodes with improved error handling and progress reporting
        /// </summary>
        /// <param name="DatabaseBranch">The parent database branch</param>
        /// <param name="dataSource">The data source containing entities</param>
        /// <param name="tree">The tree interface</param>
        /// <param name="Visutil">Visual utility manager</param>
        /// <param name="passedArgs">Progress arguments</param>
        /// <param name="dataSourceName">Name of the data source</param>
        /// <param name="refreshMode">Whether this is a refresh operation</param>
        public static async Task<IErrorsInfo> CreateEntityNodes(IBranch DatabaseBranch, IDataSource dataSource, 
            ITree tree, IAppManager Visutil, PassedArgs passedArgs, string dataSourceName, bool refreshMode = false)
        {
            try
            {
                passedArgs.Messege = "Getting entity list from data source";
                Visutil.PasstoWaitForm(passedArgs);

                // Get the list of entities
                dataSource.GetEntitesList();
                int totalEntities = dataSource.EntitiesNames?.Count() ?? 0;

                if (totalEntities == 0)
                {
                    passedArgs.Messege = "No entities found in data source";
                    Visutil.PasstoWaitForm(passedArgs);
                    return new ErrorsInfo { Flag = Errors.Ok, Message = "No entities found" };
                }

                passedArgs.Messege = $"Processing {totalEntities} entities";
                Visutil.PasstoWaitForm(passedArgs);

                int processedCount = 0;
                var createdEntities = new List<string>();
                var skippedEntities = new List<string>();
                var errorEntities = new List<string>();

                foreach (string entityName in dataSource.EntitiesNames)
                {
                    try
                    {
                        processedCount++;
                        passedArgs.Messege = $"Processing entity {processedCount}/{totalEntities}: {entityName}";
                        Visutil.PasstoWaitForm(passedArgs);

                        // Check if entity node already exists (unless refreshing)
                        if (!refreshMode && tree.Branches.Any(x => 
                            x.BranchText.Equals(entityName, StringComparison.InvariantCultureIgnoreCase) && 
                            x.ParentBranchID == DatabaseBranch.ID))
                        {
                            skippedEntities.Add(entityName);
                            continue;
                        }

                        // Get entity structure
                        var entityStructure = GetEntityStructureWithRetry(dataSource, entityName);
                        if (entityStructure == null)
                        {
                            errorEntities.Add(entityName);
                            continue;
                        }

                        // Determine icon based on entity state
                        string iconImage = entityStructure.IsCreated ? "databaseentities.ico" : "entitynotcreated.ico";

                        // Create entity node
                        var entityNode = new InMemoryEntityNode(tree, DatabaseBranch.DMEEditor, DatabaseBranch, 
                            entityName, tree.SeqID, EnumPointType.Entity, iconImage, dataSource);
                        
                        entityNode.DataSourceName = dataSource.DatasourceName;
                        entityNode.DataSource = dataSource;

                        tree.Treebranchhandler.AddBranch(DatabaseBranch, entityNode);
                        createdEntities.Add(entityName);

                        // Small delay to prevent UI freezing
                        if (processedCount % 10 == 0)
                        {
                            await Task.Delay(10);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorEntities.Add(entityName);
                        DatabaseBranch.DMEEditor?.AddLogMessage("Warning", 
                            $"Failed to process entity '{entityName}': {ex.Message}", 
                            DateTime.Now, 0, "CreateEntityNodes", Errors.Ok);
                    }
                }

                // Save entity information
                if (createdEntities.Any())
                {
                    try
                    {
                        var datasourceEntities = new DatasourceEntities 
                        { 
                            datasourcename = dataSourceName, 
                            Entities = dataSource.Entities 
                        };
                        DatabaseBranch.DMEEditor?.ConfigEditor?.SaveDataSourceEntitiesValues(datasourceEntities);
                    }
                    catch (Exception ex)
                    {
                        DatabaseBranch.DMEEditor?.AddLogMessage("Warning", 
                            $"Failed to save entity information: {ex.Message}", 
                            DateTime.Now, 0, "CreateEntityNodes", Errors.Ok);
                    }
                }

                // Report summary
                var summary = $"Completed: {createdEntities.Count} created, {skippedEntities.Count} skipped, {errorEntities.Count} errors";
                passedArgs.Messege = summary;
                Visutil.PasstoWaitForm(passedArgs);

                DatabaseBranch.DMEEditor?.AddLogMessage("Success", 
                    $"Entity processing complete for '{dataSourceName}': {summary}", 
                    DateTime.Now, 0, "CreateEntityNodes", Errors.Ok);

                return new ErrorsInfo { Flag = Errors.Ok, Message = summary };
            }
            catch (Exception ex)
            {
                DatabaseBranch.DMEEditor?.AddLogMessage("Error", 
                    $"Failed to create entity nodes: {ex.Message}", 
                    DateTime.Now, -1, "CreateEntityNodes", Errors.Failed);
                return new ErrorsInfo { Flag = Errors.Failed, Message = ex.Message };
            }
        }

        #region Private Helper Methods
        /// <summary>
        /// Processes entity refresh with child node removal and recreation
        /// </summary>
        private static async Task ProcessEntityRefresh(IBranch DatabaseBranch, IDataSource dataSource, 
            ITree tree, IAppManager Visutil, PassedArgs passedArgs, string dataSourceName)
        {
            try
            {
                passedArgs.Messege = "Connection successful - starting refresh";
                Visutil.PasstoWaitForm(passedArgs);

                // Remove existing child branches
                passedArgs.Messege = "Removing existing entities";
                Visutil.PasstoWaitForm(passedArgs);
                tree.Treebranchhandler.RemoveChildBranchs(DatabaseBranch);

                // Create new entity nodes
                await CreateEntityNodes(DatabaseBranch, dataSource, tree, Visutil, passedArgs, dataSourceName, true);

                passedArgs.Messege = "Refresh completed successfully";
                Visutil.PasstoWaitForm(passedArgs);
            }
            catch (Exception ex)
            {
                passedArgs.Messege = $"Refresh failed: {ex.Message}";
                Visutil.PasstoWaitForm(passedArgs);
                throw;
            }
        }

        /// <summary>
        /// Processes entity loading for initial load
        /// </summary>
        private static async Task ProcessEntityLoad(IBranch DatabaseBranch, IDataSource dataSource, 
            ITree tree, IAppManager Visutil, PassedArgs passedArgs, string dataSourceName)
        {
            try
            {
                passedArgs.Messege = "Connection successful - loading entities";
                Visutil.PasstoWaitForm(passedArgs);

                await CreateEntityNodes(DatabaseBranch, dataSource, tree, Visutil, passedArgs, dataSourceName, false);

                passedArgs.Messege = "Entity loading completed successfully";
                Visutil.PasstoWaitForm(passedArgs);
            }
            catch (Exception ex)
            {
                passedArgs.Messege = $"Entity loading failed: {ex.Message}";
                Visutil.PasstoWaitForm(passedArgs);
                throw;
            }
        }

        /// <summary>
        /// Gets entity structure with retry logic for improved reliability
        /// </summary>
        private static EntityStructure GetEntityStructureWithRetry(IDataSource dataSource, string entityName, int maxRetries = 3)
        {
            EntityStructure entityStructure = null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Try to get existing structure first for performance
                    if (!dataSource.Entities.Any(p => p.EntityName.Equals(entityName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        entityStructure = dataSource.GetEntityStructure(entityName, true);
                    }
                    else
                    {
                        entityStructure = dataSource.GetEntityStructure(entityName, false);
                    }

                    if (entityStructure != null)
                        break;
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        // Log final failure - can't access DMEEditor from static context, so skip logging
                        // The calling method will handle error reporting
                    }
                    else
                    {
                        // Brief delay before retry
                        Task.Delay(100).Wait();
                    }
                }
            }

            return entityStructure;
        }
        #endregion
    }
}
