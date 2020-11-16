namespace JoistArea
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Logic;
    using Services;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Plugins;
    using Tools;
    using View;
    using ViewModel;

    [Plugin(Constants.PluginName)]
    [PluginUserInterface("JoistArea.View.MainWindow")]
    [InputObjectDependency(InputObjectDependency.GEOMETRICALLY_DEPENDENT)]
    public class JoistAreaPlugin : PluginBase
    {
        private readonly JoistAreaData _uiData;

        public JoistAreaPlugin(JoistAreaData data)
        {
            _uiData = data;
        }

        public override List<InputDefinition> DefineInput()
        {
            try
            {
                //Get polygon area
                var result = new List<InputDefinition>();
                var pickedPts = PickerService.PickPointsEx(GlobalServices.GetTranslated(Constants.PickArea));
                if (pickedPts == null) return new List<InputDefinition>();

                //Get guideline area
                var guidelineSet = PickerService.PickTwoPoints(Constants.PickGuideLine);
                if (guidelineSet == null || guidelineSet.Item1 == null || guidelineSet.Item2 == null)
                    return new List<InputDefinition>();

                //Store input to memory and return
                result.Add(new InputDefinition(pickedPts));
                result.Add(new InputDefinition(new ArrayList {guidelineSet.Item1, guidelineSet.Item2}));
                return result;
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return new List<InputDefinition>();
            }
        }

        public override bool Run(List<InputDefinition> input)
        {
            //Get input from plugin instance
            if (input == null || input.Count < 2) return false;

            //Get main polygon picked points
            var pickedPts = input[0].GetInput() as ArrayList;
            if (pickedPts == null) return false;
            var pointsList = pickedPts.ToList();

            //Get guideline points to segment
            var guidePts = input[1].GetInput() as ArrayList;
            if (guidePts == null || guidePts.Count != 2) return false;
            var guideLine = new LineSegment(guidePts[0] as Point, guidePts[1] as Point);

            //Check input data and call logic function
            _uiData.CheckDefaults();

            //Custom calculate joint coordinate system and move so that logic is always run on transformed cs
            var originalPlane = new Model().GetWorkPlaneHandler().GetCurrentTransformationPlane();
            try
            {
                var cs = GetJointCoordinateSystem(pointsList, guideLine);
                if (cs == null)
                {
                    Trace.WriteLine("Unable to calculate joint coordinate system...");
                    return false;
                }
#if DEBUG
                cs.PaintCoordinateSystem();
#endif
                var transMatrix = MatrixFactory.ToCoordinateSystem(cs);
                new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane(cs));
                var logicService = new JoistAreaMainLogic();
                var transPointList = pointsList.Transform(transMatrix);
                var transGuideLine = guideLine.Transform(transMatrix);
                return logicService.CreateNew(transPointList, transGuideLine, _uiData);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return true;
            }
            finally
            {
                new Model().GetWorkPlaneHandler().SetCurrentTransformationPlane(originalPlane);
            }
        }

        /// <summary>
        /// Calculates virtual coordinate system from input points
        /// </summary>
        /// <param name="pickedPoints">Plugin picked points</param>
        /// <returns>New coordinate system based on picked points</returns>
        public static CoordinateSystem GetJointCoordinateSystem(List<Point> pickedPoints, LineSegment guideLine)
        {
            if (pickedPoints == null) throw new ArgumentNullException(nameof(pickedPoints));
            if (guideLine == null) throw new ArgumentNullException(nameof(guideLine));
            if (pickedPoints.Count < 3) return new CoordinateSystem();

            try
            {
                //Calculate coordinate system from points for polygon
                var pg = new Polygon { Points = new ArrayList(pickedPoints) };
                var gp = TxPolygon.CreatePlaneFromPolygon(pg);
                var xAxis = guideLine.GetDirectionVector().GetNormal();
                var zAxis = gp.Normal.GetNormal();
                var yAxis = Vector.Cross(zAxis, xAxis);
                return new CoordinateSystem(guideLine.Point1, xAxis, yAxis);
            }
            catch (Exception ex)
            {
                GlobalServices.LogException(ex);
                return null;
            }
        }
    }
}