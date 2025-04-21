using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep;

using TheTechIdea.Beep.DataBase;



namespace  Beep.InMemory.Nodes
{
    [AddinAttribute(Caption = "InMemory", Name = "InMemoryNode.Beep", misc = "Beep", iconimage = "category.png", menu = "Beep",ObjectType ="Beep")]
    [AddinVisSchema(BranchType = EnumPointType.Category, BranchClass = "INMEMORY")]
    public class InMemoryCategoryNode : IBranch ,IOrder 
    {
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public bool Visible { get; set; } = true;
        public InMemoryCategoryNode()
        {

        }
        public InMemoryCategoryNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranchID = pParentNode.ID;

            BranchText = pBranchText;
            BranchType = pBranchType;
           // IconImageName = pimagename;
            if (pID != 0)
            {
                ID = pID;
                BranchID = ID;
            }
        }

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
        public EnumPointType BranchType { get; set; } = EnumPointType.Category;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "inmemorycategory.png";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "INMEMORY";
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; }
        public object TreeStrucure { get; set; }
        public  IAppManager  Visutil { get; set; }
        public int MiscID { get; set; }
        public bool IsDataSourceNode { get ; set; }=false;
     
        public string MenuID { get  ; set  ; }


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

                InMemoryDatabaseNode database = new InMemoryDatabaseNode(TreeEditor, DMEEditor, this, Connection.ConnectionName, TreeEditor.SeqID, EnumPointType.DataPoint, Connection.ConnectionName);
                database.ConnectionProperties = Connection;
                TreeEditor.Treebranchhandler.AddBranch(this, database);

               // ChildBranchs.Add(database);

                //   DMEEditor.AddLogMessage("Success", "Added Database Connection", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                string mes = "Could not Add Database Connection";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };

            return DMEEditor.ErrorObject;
        }



        public IErrorsInfo CreateChildNodes()
                {

                    try
                    {
                foreach (CategoryFolder p in DMEEditor.ConfigEditor.CategoryFolders.Where(x => x.RootName == "INMEMORY" && x.FolderName == BranchText))
                {
                    foreach (string item in p.items)
                    {
                        ConnectionProperties i = DMEEditor.ConfigEditor.DataConnections.Where(x => x.ConnectionName == item).FirstOrDefault();

                        if (i != null)
                        {
                            CreateDBNode( i); //Path.Combine(i.FilePath, i.FileName)
                            i.Drawn = true;
                        }



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
        public IErrorsInfo AddFile()
        {

            try
            {
              
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

        public IBranch CreateCategoryNode(CategoryFolder p)
        {
            throw new NotImplementedException();
        }
        #endregion Exposed Interface"
        #region "Other Methods"


        #endregion"Other Methods"
    }
}
