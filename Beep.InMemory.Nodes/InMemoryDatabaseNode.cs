using Beep.Vis.Module;
using DataManagementModels.DataBase;
using System;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Util;

namespace Beep.InMemory.Nodes
{
    [AddinAttribute(Caption = "InMemory", Name = "InMemoryNode.Beep", misc = "Beep", iconimage = "inmemorydatabase.png", menu = "Beep", ObjectType = "Beep")]
    [AddinVisSchema(BranchType = EnumPointType.DataPoint, BranchClass = "INMEMORY")]
    public class InMemoryDatabaseNode : IBranch, IOrder
    {

        public InMemoryDatabaseNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranchID = pParentNode.ID;
            BranchText = pBranchText;
            BranchType = pBranchType;
            // IconImageName = pimagename;
            DataSourceName = pBranchText;
            memoryDB=DMEEditor.GetDataSource(DataSourceName) as IInMemoryDB;
            if (pID != 0)
            {
                ID = pID;
                BranchID = ID;
            }
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
        public EnumPointType BranchType { get; set; } = EnumPointType.DataPoint;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "inmemorydatabase.png";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "INMEMORY";
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; }
        public object TreeStrucure { get; set; }
        public IVisManager Visutil { get; set; }
        public int MiscID { get; set; }
        IInMemoryDB memoryDB;
        public ConnectionProperties ConnectionProperties { get; set; }
        public bool IsDataSourceNode { get; set; } = false;

        // public event EventHandler<PassedArgs> BranchSelected;
        // public event EventHandler<PassedArgs> BranchDragEnter;
        // public event EventHandler<PassedArgs> BranchDragDrop;
        // public event EventHandler<PassedArgs> BranchDragLeave;
        // public event EventHandler<PassedArgs> BranchDragClick;
        // public event EventHandler<PassedArgs> BranchDragDoubleClick;
        // public event EventHandler<PassedArgs> ActionNeeded;
        #endregion "Properties"
        #region "Interface Methods"
        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                DataSourceDefaultMethods.GetEntities(this, DMEEditor, Visutil);
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
        [CommandAttribute(Caption = "Get Entities", Hidden = false, iconimage = "getentities.png")]
        public IErrorsInfo GetEntities()
        {

            try
            {
                if(memoryDB==null)
                {
                    memoryDB = DMEEditor.GetDataSource(DataSourceName) as IInMemoryDB;
                }
                if(memoryDB==null)
                {
                    DMEEditor.AddLogMessage("Error", "Could not Get InMemory Database", DateTime.Now, -1, "Error", Errors.Failed);
                    return DMEEditor.ErrorObject;
                }
                if(memoryDB.IsStructureCreated==false)
                {
                    PassedArgs args=new PassedArgs();  
                    CancellationToken token = new CancellationToken();
                    args.Messege= $"Loadin InMemory Structure {DataSourceName}";
                    Visutil.ShowWaitForm(args);
                    Visutil.PasstoWaitForm(args);
                    var progress = new Progress<PassedArgs>(percent =>
                    {
                       
                        if(!string.IsNullOrEmpty(percent.Messege))
                        {
                            Visutil.PasstoWaitForm(percent);
                        }
                        if (percent.EventType == "Stop")
                        {
                                token.ThrowIfCancellationRequested();   
                            }
                        
                    });
                   
            
                   
                    memoryDB.LoadStructure(progress, token);
                    memoryDB.CreateStructure(progress, token);
                    //if(memoryDB.IsStructureCreated==true)
                    //{
                    //    args.Messege = $"Loading InMemory Data {DataSourceName}";
                    //    Visutil.PasstoWaitForm(args);
                    //    memoryDB.LoadData(progress, token);
                    //    memoryDB.IsLoaded = true;
                    //}
                 
                   
                }
                if(memoryDB.IsLoaded == true)
                {
                    DataSourceDefaultMethods.GetEntities(this, DMEEditor, Visutil);
                }
                Visutil.CloseWaitForm();
            }
            catch (Exception ex)
            {
                string mes = "Could not Add Database Connection";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        [CommandAttribute(Caption = "Refresh Data", Hidden = false, iconimage = "refresh.png")]
        public IErrorsInfo RefreshData()
        {

            try
            {
                if (memoryDB == null)
                {
                    memoryDB = DMEEditor.GetDataSource(DataSourceName) as IInMemoryDB;
                }
                if (memoryDB == null)
                {
                    DMEEditor.AddLogMessage("Error", "Could not Get InMemory Database", DateTime.Now, -1, "Error", Errors.Failed);
                    return DMEEditor.ErrorObject;
                }
               
                    PassedArgs args = new PassedArgs();
                    CancellationToken token = new CancellationToken();
                    args.Messege = $"Loadin Data in InMemory  {DataSourceName}";
                    Visutil.ShowWaitForm(args);
                    Visutil.PasstoWaitForm(args);
                    var progress = new Progress<PassedArgs>(percent =>
                    {

                        if (!string.IsNullOrEmpty(percent.Messege))
                        {
                            Visutil.PasstoWaitForm(percent);
                        }
                        if (percent.EventType == "Stop")
                        {
                            token.ThrowIfCancellationRequested();
                        }

                    });


                    if(memoryDB.IsStructureCreated == false)
                    {
                        args.Messege = $"Creating structure InMemory  {DataSourceName}";
                        Visutil.PasstoWaitForm(args);
                        memoryDB.LoadStructure(progress, token);
                        memoryDB.CreateStructure(progress, token);
                    }
               
                    if (memoryDB.IsStructureCreated == true)
                    {
                        args.Messege = $"Loading InMemory Data {DataSourceName}";
                        Visutil.PasstoWaitForm(args);
                        memoryDB.LoadData(progress, token);
                        memoryDB.IsLoaded = true;
                    }


               
               
                Visutil.CloseWaitForm();
                if (memoryDB.IsLoaded == false)
                {
                    DMEEditor.AddLogMessage("Error", "Could not Load InMemory Data", DateTime.Now, -1, "Error", Errors.Failed);
                    return DMEEditor.ErrorObject;
                }
               
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
            return this;
        }
        #endregion Exposed Interface"
        #region "Other Methods"


        #endregion"Other Methods"
    }
}
