namespace SpreadsheetReinforcement.Tools.Serialize
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Services;
    using Tekla.Structures.Dialog.UIControls;
    using Tekla.Structures.Model;

    public class XmlDatabase<T> : SmartBindingBase where T : IXmlData
    {
        private const string ModelSaveSubFolderName = "attributes";

        private static DirectoryInfo RootSaveDirectory
        {
            get
            {
                var rootDir = new Model().GetInfo().ModelPath;
                var di = new DirectoryInfo(Path.Combine(rootDir, ModelSaveSubFolderName));
                if (!di.Exists)
                {
                    di.Create();
                }

                return di;
            }
        }

        private static XmlDatabase<T> _instance;

        /// <summary>
        /// File extension property: must be set in Initialize (includes '.')
        /// </summary>
        public static string Extension;

        /// <summary>
        /// New singleton instance of options database service
        /// </summary>
        [XmlIgnore]
        public static XmlDatabase<T> Instance =>
            _instance ?? (_instance = new XmlDatabase<T>());

        /// <summary>
        /// Local memory cache, deserializes the first time if needed
        /// </summary>
        public ObservableCollection<T> SavedSettingCollection
        {
            get { return GetDynamicValue<ObservableCollection<T>>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary>
        /// Gets all files with extension from all standard property directories
        /// </summary>
        private List<FileInfo> AllSavedFiles
        {
            get { return GetDynamicValue<List<FileInfo>>(); }
            set { SetDynamicValue(value); }
        }

        public XmlDatabase()
        {
            SavedSettingCollection = new ObservableCollection<T>();
            AllSavedFiles = new List<FileInfo>();
        }

        /// <summary>
        /// Caches search directories and deserializes files
        /// </summary>
        public void Initialize(string extensionName)
        {
            try
            {
                Extension = extensionName;
                CacheFilesFromSearchDirectories();
                DeserializeCachedFiles();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
            }
        }

        /// <summary>
        /// Caches search directories
        /// </summary>
        private void CacheFilesFromSearchDirectories()
        {
            var result = new List<FileInfo>();
            try
            {
                //Initialize cache and get system/property directories
                var propertyDirectories = EnvironmentFiles.GetStandardPropertyFileDirectories();

                //Find files in all search directories
                foreach (var str in propertyDirectories)
                {
                    var di = new DirectoryInfo(str);
                    if (!di.Exists) continue;

                    var files = di.GetFiles(string.Format("*" + Extension), SearchOption.AllDirectories);
                    foreach (var fi in files)
                    {
                        var fileName = fi.Name;
                        var existing = result.Where(f => f.Name == fileName);
                        if (!existing.Any())
                        {
                            result.Add(fi);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalServices.LogException("Failed to cache saved setting directories.", ex);
            }
            finally
            {
                AllSavedFiles = result;
            }
        }

        /// <summary>
        /// Gets first or default setting from local memory cache by SaveName
        /// </summary>
        /// <param name="name">String name of setting</param>
        /// <returns>Setting or null if not in cache</returns>
        public T GetSettingByName(string name)
        {
            try
            {
                return SavedSettingCollection.FirstOrDefault(sett => sett.SaveName == name);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return default(T);
            }
        }

        /// <summary>
        /// Deserializes saved files to local object cache
        /// </summary>
        /// <returns>False if unable to deserialize</returns>
        private bool DeserializeCachedFiles()
        {
            var result = new ObservableCollection<T>();
            try
            {
                foreach (var fi in AllSavedFiles)
                {
                    try
                    {
                        var tempRes = ClassXmlSerializer.DeserializeFromXml<T>(fi);
                        if (tempRes == null) continue;

                        if (!result.Contains(tempRes))
                        {
                            result.Add(tempRes);
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalServices.LogException($"Failed to deserialize single file {fi.FullName}", ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException("Error: Unable to deserialize saved files", ex);
                result = new ObservableCollection<T>();
                return false;
            }
            finally
            {
                SavedSettingCollection = result;
            }
        }

        /// <summary>
        /// Re-saves all files to model folder
        /// </summary>
        /// <returns>False if not able to serialize</returns>
        public bool SerializeObjects()
        {
            if (Instance.SavedSettingCollection == null) return false;
            try
            {
                foreach (var savedSetting in Instance.SavedSettingCollection)
                {
                    if (string.IsNullOrEmpty(savedSetting.SaveName)) continue;
                    var filePath = string.Empty;
                    try
                    {
                        filePath = Path.Combine(RootSaveDirectory.FullName, savedSetting.SaveName + Extension);
                        ClassXmlSerializer.SerializeToXml(savedSetting, new FileInfo(filePath));
                    }
                    catch (Exception ex)
                    {
                        GlobalServices.LogException($"Failed to serialize single file {filePath}", ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException("Failed to serialize object.", ex);
                return false;
            }
        }

        /// <summary>
        /// Updates memory cache to include item and writes to model attributes folder
        /// </summary>
        /// <param name="newItem">Setting item</param>
        public void SaveSetting(T newItem)
        {
            try
            {
                if (newItem == null) throw new ArgumentNullException();
                if (string.IsNullOrEmpty(newItem.SaveName)) return;
                if (SavedSettingCollection == null) SavedSettingCollection = new ObservableCollection<T>();

                //Update or add to memory collection
                if (SavedSettingCollection.Contains(newItem))
                {
                    var index = SavedSettingCollection.IndexOf(newItem);
                    SavedSettingCollection[index] = newItem;
                }
                else
                {
                    SavedSettingCollection.Add(newItem);
                }

                //Overwrite file to disk
                var filePath = Path.Combine(RootSaveDirectory.FullName, newItem.SaveName + Extension);
                ClassXmlSerializer.SerializeToXml(newItem, new FileInfo(filePath));
                RaisePropertyChanged(nameof(SavedSettingCollection));
            }
            catch (Exception ex)
            {
                GlobalServices.LogException("Error: Unable to save Sort Options...", ex);
            }
        }

        /// <summary>
        /// Removes setting from memory and disk
        /// </summary>
        /// <param name="item">Setting Item</param>
        /// <returns>False if not in collection or unable to delete</returns>
        public bool RemoveSetting(T item)
        {
            try
            {
                if (item == null) throw new ArgumentNullException();
                if (SavedSettingCollection == null || !SavedSettingCollection.Contains(item))
                {
                    return false;
                }

                var res = SavedSettingCollection.Remove(item);
                RaisePropertyChanged(nameof(SavedSettingCollection));
                return res;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException("Error: Unable to save Sort Options...", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets standard or first setting from cache
        /// </summary>
        /// <returns>Cache instance of default saved setting</returns>
        public T GetDefaultSetting()
        {
            if (!SavedSettingCollection.Any()) return default(T);
            var result = Instance.GetSettingByName("standard");
            return result != null ? result : SavedSettingCollection[0];
        }
    }
}