namespace JoistArea.ViewModel
{
    using Tekla.Structures.Plugins;
    using Tools;

    public class JoistAreaData : PluginDataHelper
    {
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
            if (IsDefaultValue(CenterSpacingList)) CenterSpacingList = "900 800 750 900";
            if (IsDefaultValue(CenterSpacingMax)) CenterSpacingMax = 739.0;
            if (IsDefaultValue(JoistProfile)) JoistProfile = "650*25.0";
            if (IsDefaultValue(Class)) Class = 914;
            if (IsDefaultValue(Name)) Name = string.Empty;
            if (IsDefaultValue(Finish)) Finish = string.Empty;
            if (IsDefaultValue(PartStartNo)) PartStartNo = 0;
            if (IsDefaultValue(AssmStartNo)) AssmStartNo = 0;
            if (IsDefaultValue(PartNoPrefix)) PartNoPrefix = string.Empty;
            if (IsDefaultValue(AssmNoPrefix)) AssmNoPrefix = string.Empty;
            if (IsDefaultValue(DepthOffset)) DepthOffset = 0.0;
            if (IsDefaultValue(SpacingType)) SpacingType = 0;
            if (IsDefaultValue(FirstJoistOffset)) FirstJoistOffset = 0.0;
        }
    }
}
