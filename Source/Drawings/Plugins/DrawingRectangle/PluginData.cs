namespace DrawingRectangle
{
    using Tekla.Structures.Plugins;

    public class PluginData : PluginDataBase
    {
        [StructuresField("RectHeight")]
        public double RectHeight;

        [StructuresField("RectWidth")]
        public double RectWidth;

        public override void CheckDefaults()
        {
            base.CheckDefaults();
            if (IsDefaultValue(RectHeight)) RectHeight = 4.0 * 25.4;
            if(IsDefaultValue(RectWidth)) RectWidth = 2.0 * 25.4;
        }
    }
}
