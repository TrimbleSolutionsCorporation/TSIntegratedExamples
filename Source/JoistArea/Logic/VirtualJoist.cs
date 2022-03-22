namespace JoistArea.Logic
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tools;

    public class VirtualJoist
    {
        public Beam ModelBeam { get; set; }

        public LineSegment BeamLine => new LineSegment(ModelBeam.StartPoint, ModelBeam.EndPoint);

        public Point CenterBeam => BeamLine?.GetMidPoint();

        public VirtualJoist()
        { }

        public bool Insert()
        {
            try
            {
                if (ModelBeam == null) return false;
                return ModelBeam.Insert();
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return false;
            }
        }
    }
}
