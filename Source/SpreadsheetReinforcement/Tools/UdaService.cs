namespace SpreadsheetReinforcement.Tools
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Tekla.Structures.Catalogs;

    /// <summary>
    /// Service class to get user properties
    /// </summary>
    public class UdaService : SmartObservableObject
    {
        private ObservableCollection<UserPropertyItem>
            _userPropertyItems = new ObservableCollection<UserPropertyItem>();

        private static UdaService _instance;

        /// <summary>
        /// Singleton for UdaService
        /// </summary>
        public static UdaService Instance
        {
            get { return _instance ?? (_instance = new UdaService()); }
        }

        /// <summary> Catalog to get elements parsed from files </summary>
        private static CatalogHandler Catalog
        {
            get { return new CatalogHandler(); }
        }

        /// <summary>
        /// User property definitions from catalog helper class
        /// </summary>
        public ObservableCollection<UserPropertyItem> UserPropertyItems
        {
            get { return _userPropertyItems; }
            set { SetProperty(ref _userPropertyItems, value, () => UserPropertyItems); }
        }

        /// <summary>
        /// String list of all internal names for uda's that are defined
        /// </summary>
        public List<string> UserPropertyNames
        {
            get { return UserPropertyItems.Select(item => item.Name).ToList(); }
        }

        /// <summary>
        /// String list of all uda labels (nice string seen by user on UI)
        /// </summary>
        public List<string> UserPropertyLabels
        {
            get { return UserPropertyItems.Select(item => item.GetLabel()).ToList(); }
        }

        /// <summary>
        /// New instance of uda service class
        /// </summary>
        public UdaService()
        {
            //Needed for serialization
        }

        /// <summary>
        /// Fetches data from model, fills out cache properties on this class
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            var tempPropertyItems = new ObservableCollection<UserPropertyItem>();
            var tempEnum = Catalog.GetUserPropertyItems();
            while (tempEnum.MoveNext())
            {
                var item = tempEnum.Current;
                if (item == null) continue;
                tempPropertyItems.Add(item);
            }

            UserPropertyItems = tempPropertyItems;
        }

        /// <summary>
        /// Fetches uda definition from core cache if it exists
        /// </summary>
        /// <param name="udaName">Real Tekla name for uda</param>
        /// <returns>Returns null if not found in list of definitions</returns>
        public UserPropertyItem GetDefinition(string udaName)
        {
            return string.IsNullOrEmpty(udaName)
                ? null
                : UserPropertyItems.FirstOrDefault(item => item.Name == udaName);
        }

        /// <summary>
        /// Adds item to both internal cache lists
        /// </summary>
        /// <param name="item">Uda definition</param>
        /// <param name="modelObjects">Objects to add definition to in the model</param>
        public void AddItem(UserPropertyItem item, List<CatalogObjectTypeEnum> modelObjects = null)
        {
            if (item == null) return;
            if (modelObjects == null) modelObjects = AllPartsTypes;
            BaseAdd(item, modelObjects);
        }

        /// <summary>
        /// Adds item to both internal cache lists
        /// </summary>
        /// <param name="item">Uda Proxy item</param>
        /// <param name="modelObjects">Objects to add definition to in the model</param>
        public void AddItem(UdaPresenter item, List<CatalogObjectTypeEnum> modelObjects = null)
        {
            if (item == null) return;
            if (modelObjects == null) modelObjects = AllPartsTypes;
            BaseAdd(item.UserProperty, modelObjects);
        }

        private void BaseAdd(UserPropertyItem item, IEnumerable<CatalogObjectTypeEnum> modelObjects)
        {
            UserPropertyItems.Add(item);
            foreach (var typ in modelObjects) item.AddToObjectType(typ);
            item.Insert();
        }

        /// <summary>
        /// Removes item from both internal cache lists
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(UserPropertyItem item)
        {
            if (item == null) return;

            //Remove items from ts property list
            var newList = new ObservableCollection<UserPropertyItem>();
            foreach (var userPropertyItem in UserPropertyItems)
            {
                if (userPropertyItem.Name == item.Name) continue;
                newList.Add(userPropertyItem);
            }

            UserPropertyItems = newList;
        }

        /// <summary>
        /// Removes item from both internal cache lists
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(UdaPresenter item)
        {
            if (item == null) return;

            //Remove items from ts property list
            var newList = new ObservableCollection<UserPropertyItem>();
            foreach (var userPropertyItem in UserPropertyItems)
            {
                if (userPropertyItem.Name == item.UdaName) continue;
                newList.Add(userPropertyItem);
            }

            UserPropertyItems = newList;
        }

        /// <summary>
        /// Gets all catalog object type enum values
        /// </summary>
        public static List<CatalogObjectTypeEnum> AllPartsTypes
        {
            get
            {
                return new List<CatalogObjectTypeEnum>
                {
                    CatalogObjectTypeEnum.CONCRETE_BEAM,
                    CatalogObjectTypeEnum.CONCRETE_COLUMN,
                    CatalogObjectTypeEnum.CONCRETE_PAD_FOOTING,
                    CatalogObjectTypeEnum.CONCRETE_PANEL,
                    CatalogObjectTypeEnum.CONCRETE_SLAB,
                    CatalogObjectTypeEnum.CONCRETE_STRIP_FOOTING,
                    CatalogObjectTypeEnum.PART,
                    CatalogObjectTypeEnum.STEEL_BEAM,
                    CatalogObjectTypeEnum.STEEL_COLUMN,
                    CatalogObjectTypeEnum.STEEL_CONTOUR_PLATE,
                    CatalogObjectTypeEnum.STEEL_FOLDED_PLATE,
                    CatalogObjectTypeEnum.STEEL_ORTHOGONAL_BEAM,
                    CatalogObjectTypeEnum.STEEL_TWIN_PROFILE_BEAM,
                };
            }
        }
    }
}