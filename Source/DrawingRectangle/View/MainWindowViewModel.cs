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
            get { return this.tempName; }
            set { this.tempName = value; this.OnPropertyChanged(nameof(this.TempName)); }
        }

        [StructuresDialog("HorizOffset", typeof(TSD.Distance))]
        public TSD.Distance HorizOffset
        {
            get { return this.horizOffset; }
            set { this.horizOffset = value; this.OnPropertyChanged(nameof(this.HorizOffset)); }
        }

        [StructuresDialog("RectHeight", typeof(TSD.Distance))]
        public TSD.Distance RectHeight
        {
            get { return this.rectHeight; }
            set { this.rectHeight = value; this.OnPropertyChanged(nameof(this.RectHeight)); }
        }

        [StructuresDialog("RectWidth", typeof(TSD.Distance))]
        public TSD.Distance RectWidth
        {
            get { return this.rectWidth; }
            set { this.rectWidth = value; this.OnPropertyChanged(nameof(this.RectWidth)); }
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
