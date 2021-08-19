namespace JoistAreaFeatures.Services
{
    using System;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools.Picking;

    public static class DmCommon
    {
        /// <summary>
        /// Gets main part from Component input
        /// </summary>
        /// <param name="component">Plugin to get main part from</param>
        /// <returns>Main part</returns>
        public static Part GetCurrentMainPart(this Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            Part primaryPart = null;
            var originalInput = component.GetComponentInput();
            if (originalInput == null) return null;

            foreach (var inputItem in originalInput)
            {
                var item = inputItem as InputItem;
                if (item == null) continue;
                if (item.GetInputType() == InputItem.InputTypeEnum.INPUT_1_OBJECT)
                {
                    primaryPart = item.GetData() as Part;
                }
            }
            return primaryPart;
        }

        /// <summary>
        /// Sets attribute to component, calls Component.Modify(), then calls Model().CommitChanges()
        /// </summary>
        /// <param name="component">Base component to change settings and modify</param>
        /// <param name="attributeName">Attribute name to change</param>
        /// <param name="attributeValue">Attribute value to set</param>
        public static void ModifyComponent(this BaseComponent component, string attributeName, object attributeValue)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (!component.Identifier.IsValid()) return;
            switch (attributeValue)
            {
                case int i:
                    component.SetAttribute(attributeName, i);
                    break;

                case double d:
                    component.SetAttribute(attributeName, d);
                    break;

                default:
                    {
                        var strVal = attributeValue.ToString();
                        component.SetAttribute(attributeName, strVal);
                        break;
                    }
            }
            component.Modify();
            new Model().CommitChanges(Tekla.Structures.ModelInternal.CommitMessage.DmCommand);
        }

        /// <summary>
        /// Calls Component.Modify(), then calls Model().CommitChanges(DmCommand)
        /// </summary>
        /// <param name="component">Base component to modify</param>
        public static void ModifyComponent(this BaseComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (!component.Identifier.IsValid()) return;

            component.Modify();
            new Model().CommitChanges(Tekla.Structures.ModelInternal.CommitMessage.DmCommand);
        }

        /// <summary>
        /// Gets Tekla Model object from DM event args
        /// </summary>
        /// <param name="args">DM Event args</param>
        /// <returns>Part model object (null if not found)</returns>
        public static Part GetHitObjectPart(ToleratedObjectEventArgs args)
        {
            Part pickedPart = null;
            foreach (var hitObject in args.Objects)
            {
                pickedPart = hitObject.Object as Part;
                if (pickedPart != null) break;
            }
            return pickedPart;
        }

        /// <summary>
        /// Gets Tekla Polygon object from DM event args (from face hitObject)
        /// </summary>
        /// <param name="args">DM Event args</param>
        /// <returns>Part model object (null if not found)</returns>
        public static Polygon GetHitObjectFacePolygon(ToleratedObjectEventArgs args)
        {
            Polygon pickedFace = null;
            foreach (var hitObject in args.Faces)
            {
                pickedFace = hitObject.Object;
                if (pickedFace != null) break;
            }
            return pickedFace;
        }
    }
}
