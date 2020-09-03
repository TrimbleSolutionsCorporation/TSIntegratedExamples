namespace SpreadsheetReinforcement.Tools
{
    using System.Collections.Generic;
    using System.Linq;
    using Tekla.Structures.Catalogs;

    /// <summary>
    /// Catalog handler tools
    /// </summary>
    public static class TxCatalogService
    {
        /// <summary>
        /// Tekla catalog instance
        /// </summary>
        public static CatalogHandler CatalogHandler => new CatalogHandler();

        /// <summary>
        /// All component items not sorted
        /// </summary>
        public static List<ComponentItem> ComponentItems
        {
            get
            {
                var finalList = new List<ComponentItem>();
                var items = CatalogHandler.GetComponentItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                //finalList.Sort();
                return finalList;
            }
        }

        /// <summary>
        /// Gets specific component item from catalog by name key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ComponentItem GetComponent(string name)
        {
            foreach (var ci in ComponentItems)
            {
                //Debug.WriteLine(string.Format("Name: {0}, UI Name: {1}", ci.Name, ci.UIName));
                if (ci.Name.Contains(name)) return ci;
            }

            return null;
        }

        /// <summary>
        /// Gets string of names with prefix of type
        /// </summary>
        public static List<string> ComponentExpandedNames
        {
            get
            {
                const int maxCompNumber = 1000;
                var finalList = new List<string>();
                var items = CatalogHandler.GetComponentItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    switch (tItem.Type)
                    {
                        case ComponentItem.ComponentTypeEnum.DETAIL:
                            if (tItem.Number > maxCompNumber) break;
                            if (tItem.Number < 1)
                                finalList.Add(string.Format("{0} : {1}", "Custom Detail", tItem.UIName));
                            else finalList.Add(NormalFormat(tItem));
                            break;
                        case ComponentItem.ComponentTypeEnum.CONNECTION:
                            if (tItem.Number > maxCompNumber) break;
                            if (tItem.Number < 1)
                                finalList.Add(string.Format("{0} : {1}", "Custom Connection", tItem.UIName));
                            else finalList.Add(NormalFormat(tItem));
                            break;
                        case ComponentItem.ComponentTypeEnum.SEAM:
                            if (tItem.Number > maxCompNumber) break;
                            if (tItem.Number < 1)
                                finalList.Add(string.Format("{0} : {1}", "Custom Seam", tItem.UIName));
                            else finalList.Add(NormalFormat(tItem));
                            break;
                        case ComponentItem.ComponentTypeEnum.COMPONENT:
                            if (tItem.Number > maxCompNumber) break;
                            if (tItem.Number == -100000)
                                finalList.Add(string.Format("{0} : {1}", "Plugin", tItem.UIName));
                            else if (tItem.Number < 1)
                                finalList.Add(string.Format("{0} : {1}", "Custom Component", tItem.UIName));
                            else finalList.Add(NormalFormat(tItem));
                            break;
                        case ComponentItem.ComponentTypeEnum.UNKNOWN:
                            finalList.Add(string.Format("{0} : {1}", "Unknown", tItem.UIName));
                            break;
                    }
                }

                finalList.Sort();
                return finalList;
            }
        }

        /// <summary>
        /// All component UI (label) names sorted
        /// </summary>
        public static List<string> ComponentNames
        {
            get
            {
                var finalList = new List<string>();
                var items = CatalogHandler.GetComponentItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem.UIName);
                }

                finalList.Sort();
                return finalList;
            }
        }

        private static string NormalFormat(ComponentItem tItem)
        {
            return string.Format("{0} : {1}", tItem.UIName, tItem.Number);
        }

        /// <summary>
        /// List of all bolt items
        /// </summary>
        public static List<BoltItem> BoltItems
        {
            get
            {
                var finalList = new List<BoltItem>();
                var items = CatalogHandler.GetBoltItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Fixed profile items
        /// </summary>
        public static List<ProfileItem> FixedProfileItems
        {
            get
            {
                var finalList = new List<ProfileItem>();
                var items = CatalogHandler.GetLibraryProfileItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Parametric profile items
        /// </summary>
        public static List<ProfileItem> ParametricProfileItems
        {
            get
            {
                var finalList = new List<ProfileItem>();
                var items = CatalogHandler.GetParametricProfileItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Mesh catalog items
        /// </summary>
        public static List<MeshItem> MeshItems
        {
            get
            {
                var finalList = new List<MeshItem>();
                var items = CatalogHandler.GetMeshItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Printer catalog items
        /// </summary>
        public static List<PrinterItem> PrinterItems
        {
            get
            {
                var finalList = new List<PrinterItem>();
                var items = CatalogHandler.GetPrinterItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Gets rebar catalog items
        /// </summary>
        public static List<RebarItem> RebarItems
        {
            get
            {
                var finalList = new List<RebarItem>();
                var items = CatalogHandler.GetRebarItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Gets user property items
        /// </summary>
        public static List<UserPropertyItem> UserPropertyItems
        {
            get
            {
                var finalList = new List<UserPropertyItem>();
                var items = CatalogHandler.GetUserPropertyItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Gets material catalog items
        /// </summary>
        public static List<MaterialItem> MaterialItems
        {
            get
            {
                var finalList = new List<MaterialItem>();
                var items = CatalogHandler.GetMaterialItems();
                while (items.MoveNext())
                {
                    var tItem = items.Current;
                    if (tItem == null) continue;
                    finalList.Add(tItem);
                }

                return finalList;
            }
        }

        /// <summary>
        /// Fetches rebar item from catalog instance
        /// </summary>
        /// <param name="size"></param>
        /// <param name="grade"></param>
        /// <returns></returns>
        public static RebarItem GetRebarItem(string size, string grade)
        {
            if (string.IsNullOrEmpty(size) || string.IsNullOrEmpty(grade)) return null;
            return RebarItems.FirstOrDefault(rebarItem => rebarItem.Size == size && rebarItem.Grade == grade);
        }
    }
}