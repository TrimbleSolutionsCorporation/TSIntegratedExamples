namespace JoistArea.Tools
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Tekla.Structures.Geometry3d;
    using Tekla.Structures.Model;
    using Tekla.Structures.Model.UI;

    /// <summary>
    /// Class to pick objects from the model safely
    /// </summary>
    public class PickerService
    {
        /// <summary>Tekla Structures Model Internal Picker </summary>
        private static readonly Picker ModelPicker = new Picker();

        public static List<T> PickObjects<T>(string prompt = null) where T : ModelObject
        {
            ModelObjectEnumerator pick = null;
            var result = new List<T>();
            try
            {
                while (pick == null)
                {
                    if (prompt == null) pick = ModelPicker.PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS);
                    else pick = ModelPicker.PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS, prompt);
                    while (pick != null && pick.MoveNext())
                    {
                        var item = pick.Current as T;
                        if (item != null) result.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                else Trace.WriteLine(ex.Message);
            }
            return result;
        }

        public static T PickAnObject<T>(string prompt = null) where T : ModelObject
        {
            T pick = null;
            try
            {
                while (pick == null)
                {
                    if (prompt == null) pick = ModelPicker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT) as T;
                    else pick = ModelPicker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, prompt) as T;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                else Trace.WriteLine(ex.Message);
            }

            return pick;
        }

        public static Polygon PickAPolygon(string prompt = null)
        {
            ArrayList pick = null;
            Polygon polygon = null;
            try
            {
                while (pick == null)
                {
                    if (prompt == null) pick = ModelPicker.PickPoints(Picker.PickPointEnum.PICK_POLYGON);
                    else pick = ModelPicker.PickPoints(Picker.PickPointEnum.PICK_POLYGON, prompt);
                }

                if (pick.Count >= 2)
                {
                    polygon = new Polygon();
                    polygon.Points.AddRange(pick);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                else Trace.WriteLine(ex.Message);
            }
            return polygon;
        }

        public static Point PickAPoint(string prompt = null)
        {
            Point pick = null;
            try
            {
                pick = (prompt == null) ? ModelPicker.PickPoint() : ModelPicker.PickPoint(prompt);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                else Trace.WriteLine(ex.Message);
            }
            return pick;
        }

        public static Tuple<Point, Point> PickTwoPoints(string prompt = null)
        {
            var tuple = new Tuple<Point, Point>(null, null);
            try
            {
                var pick = (prompt == null) ? ModelPicker.PickPoints(Picker.PickPointEnum.PICK_TWO_POINTS) : ModelPicker.PickPoints(Picker.PickPointEnum.PICK_TWO_POINTS, prompt);
                foreach (var t in pick)
                {
                    var point = t as Point;
                    if (point != null && tuple.Item1 == null) tuple = new Tuple<Point, Point>(point, null);
                    else if (point != null && tuple.Item2 == null)
                    {
                        tuple = new Tuple<Point, Point>(tuple.Item1, point);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                else Trace.WriteLine(ex.Message);
            }
            return tuple;
        }

        public static List<Part> GetPartsFromModel(string prompt = "Pick parts from model")
        {
            var result = new List<Part>();
            try
            {
                var pickedObjects = ModelPicker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, prompt);
                while (pickedObjects.MoveNext())
                {
                    if (pickedObjects.Current is Part)
                        result.Add(pickedObjects.Current as Part);
                }
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public static Part GetOnePartFromModel(string prompt = "Pick part from model")
        {
            try
            {
                var result = ModelPicker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, prompt);
                return result as Part;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                return null;
            }
        }

        public static ModelObject GetOneObjectFromModel(string prompt = "Pick object from model")
        {
            try
            {
                var result = ModelPicker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, prompt);
                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt")) { }
                return null;
            }
        }

        public static List<ModelObject> GetObjectsFromModel(string prompt = "Pick objects from model")
        {
            var result = new List<ModelObject>();
            try
            {
                var pickedObjects = ModelPicker.PickObjects(Picker.PickObjectsEnum.PICK_N_PARTS, prompt);
                while (pickedObjects.MoveNext()) result.Add(pickedObjects.Current);
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }


        public static ArrayList PickPointsEx(string prompt = null)
        {
            ArrayList pickedPts = null;
            try
            {
                pickedPts = prompt == null ? ModelPicker.PickPoints(Picker.PickPointEnum.PICK_POLYGON) :
                    ModelPicker.PickPoints(Picker.PickPointEnum.PICK_POLYGON, prompt);
                return pickedPts;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("interrupt"))
                {
                    return pickedPts;
                }
                Trace.WriteLine(ex.Message);
                return null;
            }
        }
    }
}