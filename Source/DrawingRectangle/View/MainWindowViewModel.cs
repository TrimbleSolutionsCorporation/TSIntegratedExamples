namespace DrawingRectangle.View
{
    using System.ComponentModel;
    using Tekla.Structures.Dialog;
    using TSD = Tekla.Structures.Datatype;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private TSD.Distance rectHeight;
        private TSD.Distance rectWidth;
        private TSD.Distance horizOffset;
        private string tempName;

        [StructuresDialog("TempName", typeof(TSD.String))]
        public string TempName
        {
            get { return tempName; }
            set { tempName = value; OnPropertyChanged(nameof(TempName)); }
        }

        [StructuresDialog("HorizOffset", typeof(TSD.Distance))]
        public TSD.Distance HorizOffset
        {
            get { return horizOffset; }
            set { horizOffset = value; OnPropertyChanged(nameof(HorizOffset)); }
        }

        [StructuresDialog("RectHeight", typeof(TSD.Distance))]
        public TSD.Distance RectHeight
        {
            get { return rectHeight; }
            set { rectHeight = value; OnPropertyChanged(nameof(RectHeight)); }
        }

        [StructuresDialog("RectWidth", typeof(TSD.Distance))]
        public TSD.Distance RectWidth
        {
            get { return rectWidth; }
            set { rectWidth = value; OnPropertyChanged(nameof(RectWidth)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
