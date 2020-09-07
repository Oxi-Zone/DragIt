using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Multi_Compte_Dofus.Extensions
{
    // SOURCE STACKOVERFLOW url : https://stackoverflow.com/questions/16384903/moving-a-control-by-dragging-it-with-the-mouse-in-c-sharp AND Oxizone
    public static class ComponentsExtensions
    {
        //Management of mouse drag and drop
        #region Menu and Mouse
        /// <summary>
        /// First Parameter is the control who change with the control you move and the second Parameter is the control you drag
        /// </summary>
        public static event Action<Control, Control> Dragged;

        private static bool mouseDown;
        private static Point lastLocation;
        private static Point lastControlLocation;
        private static Control ControlCopy;
        private static int MinX;
        private static int MaxX;
        private static int MinY;
        private static int MaxY;
        private static string[] propertiesToCopy = null;


        public static void SetPropertiesToCopy<T>(this T control, string[] propertiesNames) where T : Control
        {
            propertiesToCopy = propertiesNames;
        }

        public static void ResetPropertiesToCopy<T>(this T control) where T : Control
        {
            propertiesToCopy = null;
        }

        public static void AddPropertyToCopy<T>(this T control, string propertyName) where T : Control
        {
            propertiesToCopy.ToList().Add(propertyName);
        }

        private static void CloneControl(Control SourceControl, Control DestinationControl)
        {
            PropertyInfo[] controlProperties = SourceControl.GetType().GetProperties();

            foreach (String Property in propertiesToCopy)
            {
                PropertyInfo ObjPropertyInfo = controlProperties.FirstOrDefault(a => a.Name == Property);
                if (ObjPropertyInfo != null)
                    ObjPropertyInfo.SetValue(DestinationControl, ObjPropertyInfo.GetValue(SourceControl));
            }
        }

        /// <summary>
        /// Update min / max limits, to disable min / max just use -1
        /// </summary>
        public static void UpdateLimits<T>(this T control, int minX = -1, int maxX = -1, int minY = -1, int maxY = -1) where T : Control
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }


        /// <summary>
        /// To enable control to be moved around with mouse, to disable min / max just use -1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        public static void moveItselfWithMouse<T>(this T control, bool lockX, bool lockY, int minX = -1, int maxX = -1, int minY = -1, int maxY = -1) where T : Control
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            control.MouseDown += (o, e) => {
                mouseDown = true;
                lastLocation = e.Location;
                lastControlLocation = control.Location;
            };

            control.MouseMove += (o, e) =>
            {
                if (mouseDown)
                {
                    if (ControlCopy == null)
                    {
                        ControlCopy = (T)Activator.CreateInstance(typeof(T));
                        CloneControl(control, ControlCopy);
                        control.FindForm().Controls.Add(ControlCopy);
                    }
                    ControlCopy.BringToFront();

                    var Xdestination = control.Location.X;
                    var Ydestination = control.Location.Y;

                    if (!lockX)
                    {
                        Xdestination = (control.Location.X - lastLocation.X) + e.X;

                        // LIMIT
                        if (MinX != -1 && Xdestination < MinX)
                            Xdestination = MinX;
                        else if (MaxX != -1 && Xdestination > MaxX)
                            Xdestination = MaxX;
                    }

                    if (!lockY)
                    {
                        Ydestination = (control.Location.Y - lastLocation.Y) + e.Y;

                        // LIMIT
                        if (MinY != -1 && Ydestination < MinY)
                            Xdestination = MinY;
                        else if (MaxY != -1 && Ydestination > MaxY)
                            Ydestination = MaxY;
                    }


                    ControlCopy.Location = new Point(Xdestination, Ydestination);
                    ControlCopy.Update();
                }
            };
            control.MouseUp += (o, e) => {
                mouseDown = false;

                if (ControlCopy == null)
                    return;

                List<T> FormElements = new List<T>();
                foreach (var cont in control.FindForm().Controls.OfType<T>().Where(x => x != control && x != ControlCopy && x.Visible && x.Location.Y == control.Location.Y))
                {
                    FormElements.Add(cont);
                }

                T element = null;
                if (lastControlLocation.X <= ControlCopy.Location.X)
                {
                    element = FormElements.OrderBy(x => x.Location.X).FirstOrDefault(x => x.Location.X + x.Size.Width > ControlCopy.Location.X + ControlCopy.Size.Width && ControlCopy.Location.X + ControlCopy.Size.Width > (x.Location.X + x.Size.Width) / 1.4d);
                }
                else
                {
                    element = FormElements.OrderByDescending(x => x.Location.X).FirstOrDefault(x => x.Location.X + x.Size.Width < ControlCopy.Location.X + ControlCopy.Size.Width && ControlCopy.Location.X < (x.Location.X + x.Size.Width) / 1.1d);
                }

                if (element != null)
                {
                    var newLocation = element.Location;
                    element.Location = new Point(control.Location.X, element.Location.Y);
                    control.Location = new Point(newLocation.X, control.Location.Y);
                    element.Update();
                    control.Update();
                    Dragged?.Invoke(element, control);
                }

                if (ControlCopy != null)
                {
                    control.FindForm().Controls.Remove(ControlCopy);
                    ControlCopy = null;
                }
            };
        }

        #endregion
    }
}
