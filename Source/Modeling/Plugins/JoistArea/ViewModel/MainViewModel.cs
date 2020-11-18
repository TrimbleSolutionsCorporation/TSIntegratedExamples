namespace JoistArea.ViewModel
{
    using System.Collections.Generic;
    using Tekla.Structures.Dialog;
    using Tools;
    using TD = Tekla.Structures.Datatype;

    public class MainViewModel : SmartBindingBase
    {
        [StructuresDialog("CenterSpacingList", typeof(TD.DistanceList))]
        public TD.DistanceList CenterSpacingList
        {
            get { return GetDynamicValue<TD.DistanceList>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("CenterSpacingMax", typeof(TD.Distance))]
        public TD.Distance CenterSpacingMax
        {
            get { return GetDynamicValue<TD.Distance>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("JoistProfile", typeof(TD.String))]
        public string JoistProfile
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("Class", typeof(TD.Integer))]
        public int Class
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        [StructuresDialog("Material", typeof(TD.String))]
        public string Material
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("Finish", typeof(TD.String))]
        public string Finish
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("PartStartNo", typeof(TD.Integer))]
        public int PartStartNo
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        [StructuresDialog("PartNoPrefix", typeof(TD.String))]
        public string PartNoPrefix
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("AssmStartNo", typeof(TD.Integer))]
        public int AssmStartNo
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        [StructuresDialog("AssmNoPrefix", typeof(TD.String))]
        public string AssmNoPrefix
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        [StructuresDialog("DepthOffset", typeof(TD.Double))]
        public double DepthOffset
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        [StructuresDialog("SpacingType", typeof(TD.Integer))]
        public int SpacingType
        {
            get { return GetDynamicValue<int>(); }
            set
            {
                SetDynamicValue<int>(value); 
                RaisePropertyChanged(nameof(IsSpacingListType));
            }
        }

        public IEnumerable<string> AllSpacingTypes => EnumTools.EnumToTranslatedStrings<SpacingTypeEnum>();

        public bool IsSpacingListType
        {
            get { return SpacingType == 0; }
        }

        public enum SpacingTypeEnum
        {
            albl_ExactList,
            albl_CenterToCenter
        }
    }
}
