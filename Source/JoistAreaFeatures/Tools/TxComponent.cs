namespace JoistAreaFeatures.Tools
{
    using System;
    using Tekla.Structures.Model;

    public static class TxComponent
    {
        public static Part GetPrimaryPart(this Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            Part result = null;

            var ci = component.GetComponentInput();
            if (ci == null) return null;

            foreach (var inputItem in ci)
            {
                var item = inputItem as InputItem;
                if (item == null) continue;
                if (item.GetInputType() != InputItem.InputTypeEnum.INPUT_1_OBJECT) continue;
                var primaryPt = item.GetData() as Part;
                if (primaryPt == null) continue;
                result = primaryPt;
            }
            return result;
        }
    }
}
