namespace JoistArea.Proxy
{
    using System;
    using Services;
    using Tekla.Structures.Model;

    public class VirtualJoist
    {
        public Beam ModelBeam { get; set; }

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
