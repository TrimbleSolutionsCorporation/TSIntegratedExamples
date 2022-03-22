namespace DrawingRectangle
{
    using Tekla.Structures.Plugins;
    using Tools;

    public class PluginData : PluginDataHelper
    {
        [StructuresField("HorizOffset")]
        public double HorizOffset;

        [StructuresField("TempName")]
        public string TempName;

        [StructuresField("RectHeight")]
        public double RectHeight;

        [StructuresField("RectWidth")]
        public double RectWidth;

        public override void CheckDefaults()
        {
            if (IsDefaultValue(this.RectHeight)) this.RectHeight = 4.0 * 25.4;
            if(IsDefaultValue(this.RectWidth)) this.RectWidth = 2.0 * 25.4;
            if (IsDefaultValue(this.HorizOffset)) this.HorizOffset = 0.0;
            if (IsDefaultValue(this.TempName)) this.TempName = string.Empty;
        }
    }
}
