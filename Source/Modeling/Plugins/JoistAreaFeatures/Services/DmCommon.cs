namespace JoistAreaFeatures.Services
{
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Services.Tools.Picking;

    public static class DmCommon
    {
        public static Part GetCurrentMainPart(this Component component)
        {
            Part primaryPart = null;
            var originalInput = component.GetComponentInput();
            if (originalInput == null) return null;

            foreach (var inputItem in originalInput)
            {
                var item = inputItem as InputItem;
                if (item == null) continue;

                switch (item.GetInputType())
                {
                    case InputItem.InputTypeEnum.INPUT_1_OBJECT:
                        primaryPart = item.GetData() as Part;
                        break;
                }
            }
            return primaryPart;
        }

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

        public static Polygon GetHitObjectFacePolygon(ToleratedObjectEventArgs eventArgs)
        {
            Polygon pickedFace = null;
            foreach (var hitObject in eventArgs.Faces)
            {
                pickedFace = hitObject.Object as Polygon;
                if (pickedFace != null) break;
            }

            return pickedFace;
        }
    }
}
