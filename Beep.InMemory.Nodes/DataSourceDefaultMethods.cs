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
    public static class DataSourceDefaultMethods
    {
        public static IErrorsInfo RefreshEntities(IBranch DatabaseBranch,IDMEEditor DMEEditor, IVisManager Visutil)
        {
            ITree tree = (ITree)Visutil.Tree;
            string BranchText = DatabaseBranch.BranchText;
            string DataSourceName = DatabaseBranch.DataSourceName;
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            PassedArgs passedArgs = new PassedArgs { DatasourceName = BranchText };
            try
            {
                string iconimage;
                IDataSource DataSource = DMEEditor.GetDataSource(BranchText);
                if (DataSource != null)
                {

                    Visutil.ShowWaitForm(passedArgs);
                    if (DataSource.ConnectionStatus != System.Data.ConnectionState.Open)
                    {
                        DataSource.Openconnection();
                    }
                    if (DataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                    {
                        if (Visutil.Controlmanager.InputBoxYesNo("Beep DM", "Are you sure, this might take some time?") ==DialogResult.Yes)
                        {
                            passedArgs.Messege = "Connection Successful";
                            Visutil.PasstoWaitForm(passedArgs);
                            passedArgs.Messege = "Getting Entities";
                            Visutil.PasstoWaitForm(passedArgs);
                            //DataSource.Entities.Clear();
                            //DataSource.GetEntitesList();
                            tree.treeBranchHandler.RemoveChildBranchs(DatabaseBranch);
                            int i = 0;
                            EntityStructure ent;
                            passedArgs.Messege = $"Getting {DataSource.EntitiesNames.Count} Entities Structures";
                            Visutil.PasstoWaitForm(passedArgs);
                            foreach (string tb in DataSource.EntitiesNames)
                            {
                                if (!tree.Branches.Any(x => x.BranchText.Equals(tb, StringComparison.InvariantCultureIgnoreCase) && x.ParentBranchID==DatabaseBranch.ID))
                                {
                                    passedArgs.Messege = $"Fetching {tb} Entity Structure";
                                    Visutil.PasstoWaitForm(passedArgs);
                                    if (!DataSource.Entities.Any(p => p.EntityName.Equals(tb, StringComparison.InvariantCultureIgnoreCase)))
                                    {
                                        ent = DataSource.GetEntityStructure(tb, true);
                                    }
                                    else
                                    {
                                        ent = DataSource.GetEntityStructure(tb, false);
                                    }
                                    iconimage = "databaseentities.ico";
                                    InMemoryDatabaseNode dbent = new InMemoryDatabaseNode(tree, DMEEditor, DatabaseBranch, tb, tree.SeqID, EnumPointType.Entity, iconimage);
                                    dbent.DataSourceName = DataSource.DatasourceName;
                                    dbent.DataSource = DataSource;
                                    tree.treeBranchHandler.AddBranch(DatabaseBranch, dbent);
                                    i += 1;
                                }
                            }
                            passedArgs.Messege = "Done";
                            Visutil.PasstoWaitForm(passedArgs);
                            DMEEditor.ConfigEditor.SaveDataSourceEntitiesValues(new TheTechIdea.Beep.ConfigUtil.DatasourceEntities { datasourcename = DataSourceName, Entities = DataSource.Entities });
                        }
                    }
                    else
                    {
                        passedArgs.Messege = "Could not Open Connection";
                        Visutil.PasstoWaitForm(passedArgs);
                    }
                }
                Visutil.CloseWaitForm();
            }
            catch (Exception ex)
            {
                DMEEditor.Logger.WriteLog($"Error in Connecting to DataSource ({ex.Message}) ");
                DMEEditor.ErrorObject.Flag = Errors.Failed;
                DMEEditor.ErrorObject.Ex = ex;
                passedArgs.Messege = "Could not Open Connection";
                Visutil.PasstoWaitForm(passedArgs);
                Visutil.CloseWaitForm();
            }
            return DMEEditor.ErrorObject;
        }
        public static IErrorsInfo GetEntities(IBranch DatabaseBranch, IDMEEditor DMEEditor, IVisManager Visutil)
        {
            ITree tree = (ITree)Visutil.Tree;
            string BranchText = DatabaseBranch.BranchText;
            string DataSourceName= DatabaseBranch.DataSourceName;
            DMEEditor.ErrorObject.Flag = Errors.Ok;
            PassedArgs passedArgs = new PassedArgs { DatasourceName = BranchText };
            try
            {
                string iconimage;
                IDataSource DataSource = DMEEditor.GetDataSource(BranchText);
                if (DataSource != null)
                {

                    Visutil.ShowWaitForm(passedArgs);
                    DataSource.Openconnection();
                    if (DataSource.ConnectionStatus == System.Data.ConnectionState.Open)
                    {
                       
                        passedArgs.Messege = "Connection Successful";
                        Visutil.PasstoWaitForm(passedArgs);
                        passedArgs.Messege = "Getting Entities";
                        Visutil.PasstoWaitForm(passedArgs);
                         DataSource.GetEntitesList();
                        int i = 0;
                        passedArgs.Messege = $"Getting {DataSource.EntitiesNames.Count} Entities";
                        Visutil.PasstoWaitForm(passedArgs);
                        foreach (string tb in DataSource.EntitiesNames)
                        {
                            if (!tree.Branches.Where(x => x.BranchText.Equals(tb, StringComparison.InvariantCultureIgnoreCase) && x.ParentBranchID == DatabaseBranch.ID).Any())
                            {
                                passedArgs.Messege = $"Fetching {tb} Entity Structure";
                                Visutil.PasstoWaitForm(passedArgs);
                                EntityStructure ent = DataSource.GetEntityStructure(tb, true);
                                if (ent.Created == false)
                                {
                                    iconimage = "entitynotcreated.ico";
                                }
                                else
                                {
                                    iconimage = "databaseentities.ico";
                                }
                                InMemoryEntityNode dbent = new InMemoryEntityNode(tree, DMEEditor, DatabaseBranch, tb, tree.SeqID, EnumPointType.Entity, iconimage,DataSource);
                                dbent.DataSourceName = DataSource.DatasourceName;
                                dbent.DataSource = DataSource;
                                tree.treeBranchHandler.AddBranch(DatabaseBranch, dbent);
                                i += 1;
                            }
                        }
                        passedArgs.Messege = "Done";
                        Visutil.PasstoWaitForm(passedArgs);
                            DMEEditor.ConfigEditor.SaveDataSourceEntitiesValues(new TheTechIdea.Beep.ConfigUtil.DatasourceEntities { datasourcename = DataSourceName, Entities = DataSource.Entities });
                       
                    }
                    else
                    {
                        passedArgs.Messege = "Could not Open Connection";
                        Visutil.PasstoWaitForm(passedArgs);
                    }
                }
                Visutil.CloseWaitForm();
            }
            catch (Exception ex)
            {
                DMEEditor.Logger.WriteLog($"Error in Connecting to DataSource ({ex.Message}) ");
                DMEEditor.ErrorObject.Flag = Errors.Failed;
                DMEEditor.ErrorObject.Ex = ex;
                passedArgs.Messege = "Could not Open Connection";
                Visutil.PasstoWaitForm(passedArgs);
                Visutil.CloseWaitForm();
            }
            return DMEEditor.ErrorObject;
        }
    }
}
