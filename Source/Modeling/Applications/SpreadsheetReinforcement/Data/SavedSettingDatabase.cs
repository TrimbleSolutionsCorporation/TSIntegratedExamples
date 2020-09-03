namespace SpreadsheetReinforcement.Data
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using Tools.Serialize;

    [XmlRoot("SpreadsheetReinforcement"), XmlType("SavedSetting")]
    public class SavedSetting : XmlData
    {
        public double MinClearCoverTop
        {
            get { return GetDynamicValue<double>();}
            set { SetDynamicValue<double>(value); }
        }

        public int MinBarSpacingFactor
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        public double MinClearCoverBottom
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        public double MinClearCoverSides
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        public double MaxSpacing
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        public double SpacingStepInterval
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        public BindingNameType BindingName
        {
            get { return GetDynamicValue<BindingNameType>(); }
            set { SetDynamicValue(value); }
        }

        public string BindingValue
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public int PillarComponentNumber
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        public string PillarComponentName
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public string PillarComponentSettings
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public int FootingComponentNumber
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        public string FootingComponentName
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public string FootingComponentSettings
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public string FootingFilter
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public string PillarFilter
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public string ImportFilePath
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public string RebarGrade
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        public SavedSetting()
        {
            SaveName = string.Empty;
            PillarComponentNumber = 30000086;
            PillarComponentName = "Starter bars for pillar (86)";
            PillarComponentSettings = "standard";
            FootingComponentNumber = 30000077;
            FootingComponentName = "Pad footing reinforcement (77)";
            FootingComponentSettings = "standard";
            MinClearCoverBottom = 2 * 25.4;
            MinClearCoverSides = 2 * 25.4;
            MinClearCoverTop = 2 * 25.4;
            MaxSpacing = 12 * 25.4;
            SpacingStepInterval = 0.25 * 25.4;
            MinBarSpacingFactor = 6;
            BindingName = BindingNameType.Name;
            RebarGrade = "A615-60";
        }

        public SavedSetting(SavedSetting existing)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));

            SaveName = existing.SaveName;
            BindingName = existing.BindingName;
            BindingValue = existing.BindingValue;
            FootingFilter = existing.FootingFilter;
            ImportFilePath = existing.ImportFilePath;
            PillarComponentNumber = existing.PillarComponentNumber;
            PillarComponentName = existing.PillarComponentName;
            PillarComponentSettings = existing.PillarComponentSettings;
            FootingComponentNumber = existing.FootingComponentNumber;
            FootingComponentName = existing.FootingComponentName;
            FootingComponentSettings = existing.FootingComponentSettings;
            PillarFilter = existing.PillarFilter;
            MinClearCoverBottom = existing.MinClearCoverBottom;
            MinClearCoverSides = existing.MinClearCoverSides;
            MinClearCoverTop = existing.MinClearCoverTop;
            MaxSpacing = existing.MaxSpacing;
            SpacingStepInterval = existing.SpacingStepInterval;
            MinBarSpacingFactor = existing.MinBarSpacingFactor;
            RebarGrade = existing.RebarGrade;
        }

        public enum BindingNameType { Name = 0, Mark = 1 }
    }

    public class SavedSettingDatabase : XmlDatabase<SavedSetting>
    {
        static SavedSettingDatabase()
        {
            Extension = ".srset";
        }
    }
}