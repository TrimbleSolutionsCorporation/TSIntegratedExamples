namespace DrawingRectangle
{
    using System.Collections.Generic;
    using Tekla.Structures.Drawing;
    using Tekla.Structures.Drawing.Tools;
    using Tekla.Structures.Plugins;

    [Plugin("Drawing Rectangle")]
    [PluginUserInterface("DrawingRectangle.View.MainWindow")]
    public class DrawingRectanglePlugin: DrawingPluginBase
    {
        private PluginData Data { get; set; }

        public DrawingRectanglePlugin(PluginData data)
        {
            Data = data;
        }

        public override List<InputDefinition> DefineInput()
        {
            var inputList = new List<InputDefinition>();
            var drawingHandler = new DrawingHandler();
            if(!drawingHandler.GetConnectionStatus()) return inputList;
            try
            {
                //Setup picker and get input from user
                var picker = drawingHandler.GetPicker().PickPoint("Pick a point...");
                inputList.Add(InputDefinitionFactory.CreateInputDefinition(picker.Item2, picker.Item1));
                return inputList;
            }
            catch(PickerInterruptedException)
            {
                return null;
            }
        }

        public override bool Run(List<InputDefinition> input)
        {
            //Check input
            if(input == null || input.Count < 1) return false;

            //Set defaults for attributes
            Data.CheckDefaults();

            //Get data from picker
            var viewBase = InputDefinitionFactory.GetView(input[0]);
            if(viewBase == null) return false;
            var pickedView = viewBase;
            var pickedPoint = InputDefinitionFactory.GetPoint(input[0]);

            //Main method here
            PluginLogic.RunLogic(pickedView, pickedPoint, Data);
            return false;
        }
    }
}
