namespace SpreadsheetReinforcement.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Tools;
    using ViewModel;

    public class SpreadsheetResultData : SmartBindingBase
    {
        /// <summary> Internal tracking guid </summary>
        public Guid Guid
        {
            get { return GetDynamicValue<Guid>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Joint name </summary>
        public string Mark
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Footing part name </summary>
        public string FootingName
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Bottom min area of steel for x direction </summary>
        public double BottomXSteel
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        /// <summary> Bottom min area of steel for z direction </summary>
        public double BottomZSteel
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        /// <summary> Top min area of steel for x direction </summary>
        public double TopXSteel
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        /// <summary> Top min area of steel for z direction </summary>
        public double TopZSteel
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        /// <summary> Reinforcement grade </summary>
        public string RebarGrade
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Pillaster bar quantity </summary>
        public int LongBarQty
        {
            get { return GetDynamicValue<int>(); }
            set { SetDynamicValue<int>(value); }
        }

        /// <summary> Pile bar size </summary>
        public string LongBarSize
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Stirrup bar size </summary>
        public string TransBarSize
        {
            get { return GetDynamicValue<string>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary>
        /// Calculated stirrup diameter (nominal)
        /// </summary>
        public double StirrupSize
        {
            get
            {
                try
                {
                    var barNoStr = Regex.Replace(TransBarSize, GlobalConstants.Nums, "");
                    var barNo = Convert.ToDouble(barNoStr);
                    return barNo * 0.125 * 25.4;
                }
                catch (Exception)
                {
                    return 0.0;
                }
            }
        }

        /// <summary> Stirrup spacing </summary>
        public double TransBarSpacing
        {
            get { return GetDynamicValue<double>(); }
            set { SetDynamicValue<double>(value); }
        }

        /// <summary>
        /// New instance of spreadsheet results
        /// Creates new internal guid
        /// </summary>
        public SpreadsheetResultData()
        {
            Guid = Guid.NewGuid();
        }

        /// <summary>
        /// New instance of spreadsheet results from cell contents
        /// Creates new internal guid
        /// </summary>
        /// <param name="cellList"></param>
        /// <param name="options"></param>
        public SpreadsheetResultData(IReadOnlyList<string> cellList, SavedSetting options)
        {
            if (cellList == null) throw new ArgumentNullException(nameof(cellList));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (cellList.Count != 8)
                throw new ApplicationException(
                    "DesignResult constructor fail, wrong number of columns for spreadsheet data.");

            Guid = Guid.NewGuid();
            Mark = cellList[0].Trim();
            FootingName = cellList[1].Trim();
            BottomXSteel = Convert.ToDouble(Regex.Replace(cellList[2].Trim(), GlobalConstants.Nums, "")) *
                           Math.Pow(GlobalConstants.InchToMmFactor, 2);
            BottomZSteel = Convert.ToDouble(Regex.Replace(cellList[3].Trim(), GlobalConstants.Nums, "")) *
                           Math.Pow(GlobalConstants.InchToMmFactor, 2);
            TopXSteel = Convert.ToDouble(Regex.Replace(cellList[4].Trim(), GlobalConstants.Nums, "")) *
                        Math.Pow(GlobalConstants.InchToMmFactor, 2);
            TopZSteel = Convert.ToDouble(Regex.Replace(cellList[5].Trim(), GlobalConstants.Nums, "")) *
                        Math.Pow(GlobalConstants.InchToMmFactor, 2);

            RebarGrade = options.RebarGrade;
            LongBarSize = GlobalConstants.RebarSizeSymbol +
                          Regex.Replace(cellList[6].Trim().StringRight(GlobalConstants.RebarSizeSymbol),
                              GlobalConstants.Nums, "");
            LongBarQty = Convert.ToInt32(Regex.Replace(cellList[6].Trim().StringLeft(GlobalConstants.RebarSizeSymbol),
                GlobalConstants.Nums, ""));
            TransBarSize = GlobalConstants.RebarSizeSymbol + Regex.Replace(
                               cellList[7].Trim().StringLeft(GlobalConstants.RebarSpacingSeperator),
                               GlobalConstants.Nums, "");
            TransBarSpacing =
                Convert.ToDouble(Regex.Replace(cellList[7].Trim().StringRight(GlobalConstants.RebarSpacingSeperator),
                    GlobalConstants.Nums, "")) * GlobalConstants.InchToMmFactor;
        }
    }
}