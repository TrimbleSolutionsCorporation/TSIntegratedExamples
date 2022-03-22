namespace SpreadsheetReinforcement.PartProxy
{
    using System;
    using Tekla.Structures.Model;
    using Tools;

    /// <summary>
    /// Represents a matched pair of parts at one joint location in the model
    /// Pad footing and pillar
    /// </summary>
    public class FoundationPair : SmartBindingBase, IEquatable<FoundationPair>
    {
        /// <summary>
        /// New instance of matched Pillar and pad footing at one joint location
        /// </summary>
        /// <param name="footing">Pad footing</param>
        /// <param name="pillar">Pillar</param>
        public FoundationPair(Part footing, Part pillar)
        {
            Footing = footing ?? throw new ArgumentNullException(nameof(footing));
            Pillar = pillar ?? throw new ArgumentNullException(nameof(pillar));
        }

        /// <summary> Pad footing in model </summary>
        public Part Footing
        {
            get { return GetDynamicValue<Part>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary> Pillar in model </summary>
        public Part Pillar
        {
            get { return GetDynamicValue<Part>(); }
            set { SetDynamicValue(value); }
        }

        /// <summary>
        /// Compares part temp id's
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(FoundationPair other)
        {
            if (other == null) return false;
            if (other.Footing.Identifier.ID != Footing.Identifier.ID) return false;
            return other.Pillar.Identifier.ID == Pillar.Identifier.ID;
        }
    }
}