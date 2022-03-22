namespace JoistAreaFeatures.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using JoistArea.Tools;
    using JoistArea.ViewModel;
    using Tekla.Structures.Model;

    public static class PluginDataFetcher
    {
        /// <summary>
        /// Generic method to get field values, match with passed attributes, and fill data class values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">New instance of PluginData custom class to fill</param>
        /// <param name="attributes">List of stored attributes on Component level to fill Data class from</param>
        /// <returns>Plugin Data class with filled Tekla Structures Field values</returns>
        private static T GetDataFromAttributes<T>(this T instance, Dictionary<string, object> attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));

            var typ = typeof(T);
            foreach (var classField in typ.GetFields())
            {
                if (!classField.IsPublic) continue;
                var tsPropName = GetTsCustomProp(classField);
                if (string.IsNullOrEmpty(tsPropName)) continue;

                var pName = classField.Name;
                if (!attributes.ContainsKey(pName)) continue;
                var attributeValue = attributes[pName];
                classField.SetValue(instance, attributeValue);
            }
            return instance;
        }

        /// <summary>
        /// Sets values on Component instance from passed attributes
        /// </summary>
        /// <param name="component">Model Component instance - plugin</param>
        /// <param name="currentValues">List of stored attributes</param>
        public static void SetDataFromAttributes(Component component, Dictionary<string, object> currentValues)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (currentValues == null) return;

            foreach (var kp in currentValues)
            {
                if (kp.Value is string)
                {
                    component.SetAttribute(kp.Key, kp.Value.ToString());
                }
                else if (kp.Value is int)
                {
                    var tempVal = (int)kp.Value;
                    component.SetAttribute(kp.Key, tempVal);
                }
                else if (kp.Value is double)
                {
                    var tempVal = (double)kp.Value;
                    component.SetAttribute(kp.Key, tempVal);
                }
            }
        }

        /// <summary>
        /// Specific Instance based custom Plugin data class method to fetch and then validate field values
        /// </summary>
        /// <param name="attributes">List of stored attributes on Component level to fill Data class from</param>
        /// <returns>LiftingInsertsData Data class with filled Tekla Structures Field values</returns>
        public static JoistAreaData GetDataFromAttributes(Dictionary<string, object> attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            var uiData = new JoistAreaData().GetDataFromAttributes(attributes);
            uiData.CheckDefaults();
            return uiData;
        }

        public static JoistAreaData GetDataFromComponent(this Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            var uiData = new JoistAreaData();

            var typ = typeof(JoistAreaData);
            foreach (var classField in typ.GetFields())
            {
                if (!classField.IsPublic) continue;
                var tsPropName = GetTsCustomProp(classField);
                if (string.IsNullOrEmpty(tsPropName)) continue;
                var fldTypName = classField.FieldType.Name;

                if (fldTypName=="String")
                {
                    var tempValue = string.Empty;
                    component.GetAttribute(tsPropName, ref tempValue);
                    classField.SetValue(uiData, tempValue);
                }
                else if (fldTypName=="Int32")
                {
                    var tempValue = TxModel.NullIntegerValue;
                    component.GetAttribute(tsPropName, ref tempValue);
                    classField.SetValue(uiData, tempValue);
                }
                else if (fldTypName=="Double")
                {
                    var tempValue = Convert.ToDouble(TxModel.NullIntegerValue);
                    component.GetAttribute(tsPropName, ref tempValue);
                    classField.SetValue(uiData, tempValue);
                }
            }

            uiData.CheckDefaults();
            return uiData;
        }

        /// <summary>
        /// Sets all component data from attributes individually
        /// </summary>
        /// <param name="component"></param>
        /// <param name="pluginData"></param>
        public static void SetDataFromAttributes<T>(this Component component, T pluginData)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (pluginData == null) return;

            var typ = typeof(T);
            foreach (var classField in typ.GetFields())
            {
                if (!classField.IsPublic) continue;
                var tsPropName = GetTsCustomProp(classField);
                if (string.IsNullOrEmpty(tsPropName)) continue;

                var pValue = classField.GetValue(pluginData);
                component.SetComponentValue(new KeyValuePair<string, object>(tsPropName, pValue));
            }
        }

        /// <summary>
        /// Sets component attribute with value stored as generic object
        /// </summary>
        /// <param name="component">Model component to set Attribute value for</param>
        /// <param name="kp">Key pair (AttributeName, Value)</param>
        private static void SetComponentValue(this Component component, KeyValuePair<string, object> kp)
        {
            if (kp.Value is string)
            {
                component.SetAttribute(kp.Key, kp.Value.ToString());
            }
            else if (kp.Value is int)
            {
                var tempVal = (int)kp.Value;
                component.SetAttribute(kp.Key, tempVal);
            }
            else if (kp.Value is double)
            {
                var tempVal = (double)kp.Value;
                component.SetAttribute(kp.Key, tempVal);
            }
        }

        /// <summary>
        /// Get TeklaStructures custom attribute name for field
        /// </summary>
        /// <param name="fInfo">Class Field Info</param>
        /// <returns>Tekla Structures binding name to core plugin or string.Empty if none</returns>
        private static string GetTsCustomProp(FieldInfo fInfo)
        {
            const string TsPropName = "StructuresFieldAttribute";
            foreach (var cda in fInfo.CustomAttributes)
            {
                if (cda.AttributeType.Name != TsPropName) continue;
                var cdaArgs = cda.ConstructorArguments;
                if (cdaArgs.Count < 1) return string.Empty;
                var tsConstVal = cdaArgs[0].ToString();
                return tsConstVal.Replace("\"", string.Empty);
            }
            return string.Empty;
        }
    }
}
