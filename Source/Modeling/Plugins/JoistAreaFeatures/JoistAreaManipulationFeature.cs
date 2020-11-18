namespace JoistAreaFeatures
{
    using System.Collections.Generic;
    using JoistArea.View;
    using JoistArea.ViewModel;
    using Services;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins.DirectManipulation.Core;
    using Tekla.Structures.Plugins.DirectManipulation.Core.Features;

    /// <summary>
    /// Direct Manipulation manipulation feature for the BeamPlugin class.
    /// </summary>
    /// <seealso cref="PluginManipulationFeatureBase" />
    public class JoistAreaManipulationFeature : PluginManipulationFeatureBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JoistAreaManipulationFeature"/> class.
        /// </summary>
        public JoistAreaManipulationFeature()
            : base(Constants.PluginName)
        { }

        //public override bool UseDefaultManipulator { get; } = false;

        public JoistAreaData GetUserInterfaceData(Component component)
        {
            var storedValues = GetAppliedAttributes(component);
            return PluginDataFetcher.GetDataFromAttributes(storedValues);
        }

        /// <inheritdoc />
        protected override IEnumerable<ManipulationContext> AttachManipulationContexts(Component component)
        {
            yield return new JoistAreaManipulationContext(component, this);
        }
    }
}