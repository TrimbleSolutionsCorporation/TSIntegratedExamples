namespace JoistArea.ViewModel
{
    using Tekla.Structures.Plugins;
    using Tools;

    public class JoistAreaData : PluginDataHelper
    {
        public const string DefaultCenterSpacingList = "900 800 750 900";
        public const double DefaultCenterSpacingMax = 739.0;
        public const double DefaultDepthOffset = 0.0;
        public const string DefaultJoistProfile = "650*25.0";
        public const int DefaultClass = 914;
        public const string DefaultName = "Joist";
        public const string DefaultFinish = "";
        public const int DefaultPartStartNo = 1;
        public const int DefaultAssmStartNo = 1;
        public const string DefaultPartNoPrefix = "j";
        public const string DefaultAssmNoPrefix = "J";
        public const int DefaultSpacingType = 0;
        public const double DefaultFirstJoistOffset = 0.0;

        [StructuresField("CenterSpacingList")] public string CenterSpacingList;
        [StructuresField("CenterSpacingMax")] public double CenterSpacingMax;
        [StructuresField("JoistProfile")] public string JoistProfile;
        [StructuresField("Material")] public string Material;
        [StructuresField("Class")] public int Class;
        [StructuresField("Name")] public string Name;
        [StructuresField("Finish")] public string Finish;
        [StructuresField("PartStartNo")] public int PartStartNo;
        [StructuresField("AssmStartNo")] public int AssmStartNo;
        [StructuresField("PartNoPrefix")] public string PartNoPrefix;
        [StructuresField("AssmNoPrefix")] public string AssmNoPrefix;
        [StructuresField("DepthOffset")] public double DepthOffset;
        [StructuresField("SpacingType")] public int SpacingType;
        [StructuresField("FirstJoistOffset")] public double FirstJoistOffset;

        public override void CheckDefaults()
        {
            if (IsBlankValue(CenterSpacingList)) CenterSpacingList = DefaultCenterSpacingList;
            if (IsBlankValue(CenterSpacingMax)) CenterSpacingMax = DefaultCenterSpacingMax;
            if (IsBlankValue(JoistProfile)) JoistProfile = DefaultJoistProfile;
            if (IsBlankValue(Class)) Class = DefaultClass;
            if (IsBlankValue(Name)) Name = DefaultName;
            if (IsBlankValue(Finish)) Finish = DefaultFinish;
            if (IsBlankValue(PartStartNo)) PartStartNo = DefaultPartStartNo;
            if (IsBlankValue(AssmStartNo)) AssmStartNo = DefaultAssmStartNo;
            if (IsBlankValue(PartNoPrefix)) PartNoPrefix = DefaultPartNoPrefix;
            if (IsBlankValue(AssmNoPrefix)) AssmNoPrefix = DefaultAssmNoPrefix;
            if (IsBlankValue(DepthOffset)) DepthOffset = DefaultDepthOffset;
            if (IsBlankValue(SpacingType)) SpacingType = DefaultSpacingType;
            if (IsBlankValue(FirstJoistOffset)) FirstJoistOffset = DefaultFirstJoistOffset;
        }
    }
}
