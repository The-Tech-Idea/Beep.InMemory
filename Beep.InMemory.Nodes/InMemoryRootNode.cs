using Beep.InMemory.Logic;
using Beep.Vis.Module;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Util;
using DataManagementModels.DriversConfigurations;
using System.Reflection;
using static TheTechIdea.Beep.Util;
using DataManagementModels.DataBase;
namespace  Beep.InMemory.Nodes
{
    [AddinAttribute(Caption = "InMemory", Name = "InMemoryNode.Beep", misc = "Beep", iconimage = "inmemoryroot.png", menu = "DataSource", ObjectType ="Beep", Category = DatasourceCategory.INMEMORY)]
    [AddinVisSchema(BranchType = EnumPointType.Root, BranchClass = "DATASOURCEROOT", RootNodeName = "DataSourcesRootNode")]
    public class InMemoryRootNode  : IBranch ,IOrder 
    {
        public InMemoryRootNode()
        {

        }
        public InMemoryRootNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename, string ConnectionName)
        {



            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            //ParentBranchID = pParentNode.ID;
            //BranchText = pBranchText;
            //BranchType = pBranchType;
            //IconImageName = pimagename;

            //if (pID != 0)

            //{
            //    ID = pID;
            //    BranchID = pID;
            //}
        }
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        #region "Properties"
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
        public List<string> BranchActions { get; set; }
        public object TreeStrucure { get; set; }
        public  IVisManager  Visutil { get; set; }
        public int MiscID { get; set; }
        public bool IsDataSourceNode { get ; set; }=false;


        // public event EventHandler<PassedArgs> BranchSelected;
        // public event EventHandler<PassedArgs> BranchDragEnter;
        // public event EventHandler<PassedArgs> BranchDragDrop;
        // public event EventHandler<PassedArgs> BranchDragLeave;
        // public event EventHandler<PassedArgs> BranchDragClick;
        // public event EventHandler<PassedArgs> BranchDragDoubleClick;
        // public event EventHandler<PassedArgs> ActionNeeded;
        #endregion "Properties"
        #region "Interface Methods"
        public IErrorsInfo CreateDBNode(ConnectionProperties Connection)
        {

            try
            {
                if (Connection != null)
                {
                    ConnectionDriversConfig driversConfig = DMEEditor.Utilfunction.LinkConnection2Drivers(Connection);
                    if (driversConfig != null)
                    {
                        AssemblyClassDefinition classDefinition = DMEEditor.ConfigEditor.BranchesClasses.FirstOrDefault(p => p.className.StartsWith(driversConfig.classHandler));
                        ConstructorInfo ctor=null;
                        if (classDefinition != null)
                        {
                            Type adc = DMEEditor.assemblyHandler.GetType(classDefinition.PackageName);
                            try
                            {
                                ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;
                            }
                            catch (Exception)
                            {

                            }
                           
                            ObjectActivator<IBranch> createdActivator = GetActivator<IBranch>(ctor);
                            IBranch br = createdActivator();
                            // set ConnectionProperties to the branch using reflection
                            PropertyInfo prop = br.GetType().GetProperty("ConnectionProperties");
                            prop.SetValue(br, Connection);
                            // set other properties
                            br.DMEEditor= DMEEditor;
                            br.ID = TreeEditor.SeqID;
                            br.BranchID = TreeEditor.SeqID;
                            br.ParentBranchID = ID;
                            br.BranchText = Connection.ConnectionName;
                            br.DataSourceName = Connection.ConnectionName;
                            IInMemoryDB memoryDB = DMEEditor.GetDataSource(Connection.ConnectionName) as IInMemoryDB;
                            br.DataSource = (IDataSource)memoryDB;
                            br.TreeEditor = TreeEditor;
                            PropertyInfo propmemoryDB = br.GetType().GetProperty("memoryDB");
                            propmemoryDB.SetValue(br, memoryDB);

                            TreeEditor.treeBranchHandler.AddBranch(this, br);
                        }
                        else
                        {

                            InMemoryDatabaseNode database = new InMemoryDatabaseNode(TreeEditor, DMEEditor, this, Connection.ConnectionName, TreeEditor.SeqID, EnumPointType.DataPoint, Connection.ConnectionName);
                            database.ConnectionProperties = Connection;
                            TreeEditor.treeBranchHandler.AddBranch(this, database);
                        }
                    }
                }
           }
            catch (Exception ex)
            {
                string mes = "Could not Add Database Connection";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };

            return DMEEditor.ErrorObject;
        }
        public IBranch CreateCategoryNode(CategoryFolder p)
        {
            InMemoryCategoryNode Category = null;
            try
            {
                Category = new InMemoryCategoryNode(TreeEditor, DMEEditor, this, p.FolderName, TreeEditor.SeqID, EnumPointType.Category, TreeEditor.CategoryIcon);
                TreeEditor.treeBranchHandler.AddBranch(this, Category);
                //ChildBranchs.Add(Category);
                Category.CreateChildNodes();

            }
            catch (Exception ex)
            {
                DMEEditor.Logger.WriteLog($"Error Creating Category Node File Node ({ex.Message}) ");
                DMEEditor.ErrorObject.Flag = Errors.Failed;
                DMEEditor.ErrorObject.Ex = ex;
            }
            return Category;

        }
      

        public IErrorsInfo CreateChildNodes()
                {

                    try
                    {
                        // TreeEditor.treeBranchHandler.RemoveChildBranchs(this);
                        foreach (ConnectionProperties i in DMEEditor.ConfigEditor.DataConnections.Where(c => c.IsInMemory==true && c.Category== DatasourceCategory.INMEMORY))
                        {
                            if (TreeEditor.treeBranchHandler.CheckifBranchExistinCategory(i.ConnectionName, "INMEMORY") == null)
                            {
                                if (!ChildBranchs.Any(p => p.BranchText.Equals(i.ConnectionName, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    CreateDBNode( i);
                                    i.Drawn = true;
                                }

                            }


                        }
                        foreach (CategoryFolder i in DMEEditor.ConfigEditor.CategoryFolders.Where(x => x.RootName.Equals("INMEMORY", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            if (!ChildBranchs.Where(p => p.BranchText == i.FolderName && i.RootName == "INMEMORY").Any())
                            {
                                CreateCategoryNode(i);
                            }




                        }

                        //    DMEEditor.AddLogMessage("Success", "Added Database Connection", DateTime.Now, 0, null, Errors.Ok);
                    }
                    catch (Exception ex)
                    {
                        string mes = "Could not Add Database Connection";
                        DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
                    };
                    return DMEEditor.ErrorObject;

                }

        public IErrorsInfo ExecuteBranchAction(string ActionName)
        {
            throw new NotImplementedException();
        }

        public IErrorsInfo MenuItemClicked(string ActionNam)
        {
            throw new NotImplementedException();
        }

        public IErrorsInfo RemoveChildNodes()
        {
            throw new NotImplementedException();
        }

        public IErrorsInfo SetConfig(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            try
            {
                TreeEditor = pTreeEditor;
                DMEEditor = pDMEEditor;
                //ParentBranchID = pParentNode.ID;
                //BranchText = pBranchText;
                //BranchType = pBranchType;
                //IconImageName = pimagename;
                //if (pID != 0)
                //{
                //    ID = pID;
                //}

                //   DMEEditor.AddLogMessage("Success", "Set Config OK", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                string mes = "Could not Set Config";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        #endregion "Interface Methods"
        #region "Exposed Interface"
     
        [CommandAttribute(Caption = "Add InMemory Database", Hidden = false, iconimage = "add.png")]
        public IErrorsInfo Add()
        {

            try
            {
                ConnectionProperties conn=BeepInMemoryManager.CreateInMemoryDB(DMEEditor, Visutil);
                if(DMEEditor.ErrorObject.Flag == Errors.Ok)
                {
                    if (conn != null)
                    {
                        CreateDBNode(conn);
                    }
                  
                }   
                //NodesHelpers.AddFile(this, TreeEditor, DMEEditor, Visutil);
            }
            catch (Exception ex)
            {
                string mes = "Could not Add Database Connection";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        [CommandAttribute(Caption = "Remove all", Hidden = false, iconimage = "remove.ico")]
        public IErrorsInfo remove()
        {

            try
            {
              
            }
            catch (Exception ex)
            {
                string mes = "Could not Add Database Connection";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        #endregion Exposed Interface"
        #region "Other Methods"
     
      
        #endregion"Other Methods"
    }
}
