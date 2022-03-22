namespace JoistArea.Tools
{
    using System;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Solid;

    /// <summary>
    /// Joint proxy element
    /// </summary>
    public class FaceDifference
    {
        /// <summary>
        /// Primary face of joint
        /// </summary>
        public Face PrimaryFace { get; set; }

        /// <summary>
        /// Secondary face of joint
        /// </summary>
        public Face SecondaryFace { get; set; }

        /// <summary>
        /// Determines if this is a valid joint between two part faces
        /// </summary>
        public bool IsValidJoint { get; private set; }


        /// <summary>
        /// Geometric plane at center of two faces with normal same as primary face
        /// </summary>
        public GeometricPlane JointCenterPlane
        {
            get
            {
                var transOrigin = new Point(PrimaryFace.GetFaceOrigin());
                transOrigin.Translate(PrimaryFace.Normal * (Thickness * 0.5));
                return new GeometricPlane(transOrigin, PrimaryFace.Normal);
            }
        }

        /// <summary>
        /// Joint gap parallel to primary face normal
        /// </summary>
        public double Thickness
        {
            get { return Math.Abs(new Vector(SecondaryFace.GetFaceOrigin() - PrimaryFace.GetFaceOrigin()).Dot(PrimaryFace.Normal)); }
        }

        /// <summary>
        /// New joint instance between two faces
        /// </summary>
        /// <param name="primaryFace">Primary part face</param>
        /// <param name="secondaryFace">Secondary part face</param>
        public FaceDifference(Face primaryFace, Face secondaryFace)
        {
            if (primaryFace == null) throw new ArgumentNullException(nameof(primaryFace));
            if (secondaryFace == null) throw new ArgumentNullException(nameof(secondaryFace));
            PrimaryFace = primaryFace;
            SecondaryFace = secondaryFace;
            IsValidJoint = CheckJointValidity();
        }

        /// <summary>
        /// Checks that face normals are pointing towards each other
        /// </summary>
        /// <returns>True if valid joint</returns>
        private bool CheckJointValidity()
        {
            if (!PrimaryFace.Normal.IsOppositeDirection(SecondaryFace.Normal, 9)) return false;
            return true;
        }
    }
}
