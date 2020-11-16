namespace JoistArea.Tools
{
    using System.Collections.Generic;
    using Tekla.Structures.Model;
    using Tekla.Structures.Solid;

    /// <summary>
    /// Solid Extension Class
    /// </summary>
    public static class TxSolid
    {
        /// <summary>
        /// Enumerates the faces of a solid
        /// </summary>
        /// <param name="solid">
        /// The solid.
        /// </param>
        /// <returns>
        /// Each face of the solid.
        /// </returns>
        public static IEnumerable<Face> GetFaces(this Solid solid)
        {
            var faceList = solid.GetFaceEnumerator();
            while (faceList.MoveNext())
            {
                var face = faceList.Current;
                if (face != null) yield return face;
            }
        }
    }
}