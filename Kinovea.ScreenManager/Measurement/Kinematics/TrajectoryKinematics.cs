using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Kinovea.ScreenManager 
{
    /// <summary>
    /// Raw and filtered kinematics values in calibrated coordinates/units.
    /// </summary>
    public class TrajectoryKinematics
    {
        public double[] this[string component]
        {
            get
            {
                if (!components.ContainsKey(component))
                    throw new InvalidOperationException();

                return components[component];
            }
        }

        /// <summary>
        /// Number of samples.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Whether the trajectory can be filtered, depends on the number of samples.
        /// </summary>
        public bool CanFilter { get; private set; }

        /// <summary>
        /// Time coordinates.
        /// </summary>
        public long[] Times { get; private set; }

        /// <summary>
        /// Raw coordinates.
        /// </summary>
        public double[] RawXs { get; private set; }

        /// <summary>
        /// Raw coordinates.
        /// </summary>
        public double[] RawYs { get; private set; }

        /// <summary>
        /// Filtered coordinates at the currently selected cutoff frequency.
        /// </summary>
        public double[] Xs 
        { 
            get 
            {
                return XCutoffIndex < 0 ? RawXs : FilterResultXs[XCutoffIndex].Data;
            }
        }

        /// <summary>
        /// Filtered coordinates at the currently selected cutoff frequency.
        /// </summary>
        public double[] Ys
        {
            get
            {
                return YCutoffIndex < 0 ? RawYs : FilterResultYs[YCutoffIndex].Data;
            }
        }

        /// <summary>
        /// Filtered X coordinates time series at various cutoff frequencies.
        /// </summary>
        public List<FilteringResult> FilterResultXs { get; set; }

        /// <summary>
        /// Best-guess cutoff frequency index for Xs series.
        /// </summary>
        public int XCutoffIndex { get; set; }

        /// <summary>
        /// Filtered time series at various cutoff frequencies.
        /// </summary>
        public List<FilteringResult> FilterResultYs { get; set; }

        /// <summary>
        /// Best-guess cutoff frequency index for Ys series.
        /// </summary>
        public int YCutoffIndex { get; set; }

        //---------------------------------------------------------------------
        // Linear kinematics. All values are in calibrated coordinates/units.
        // There is always as many data points as there are point coordinates, the uncomputables values are set to NaN.
        public double[] RawTotalDistance { get; private set; }
        public double[] RawSpeed { get; private set; }
        public double[] RawVerticalVelocity { get; private set; }
        public double[] RawHorizontalVelocity { get; private set; }
        public double[] RawAcceleration { get; private set; }
        public double[] RawVerticalAcceleration { get; private set; }
        public double[] RawHorizontalAcceleration { get; private set; }

        public double[] TotalDistance { get; private set; }
        public double[] Speed { get; set; }
        public double[] VerticalVelocity { get; set; }
        public double[] HorizontalVelocity { get; set; }
        public double[] Acceleration { get; set; }
        public double[] VerticalAcceleration { get; set; }
        public double[] HorizontalAcceleration { get; set; }

        //-----------------------------------
        // Best fit circle angular kinematics
        // TODO: pass absolute angles through filtering ?
        public PointF RotationCenter { get; set; }
        public double RotationRadius { get; set; }

        public double[] AbsoluteAngle { get; private set; }
        public double[] DisplacementAngle { get; private set; }
        public double[] TotalDisplacementAngle { get; private set; }
        public double[] AngularVelocity { get; private set; }
        public double[] TangentialVelocity { get; private set; }
        public double[] CentripetalAcceleration { get; private set; }
        public double[] AngularAcceleration { get; private set; }

        private Dictionary<string, double[]> components = new Dictionary<string, double[]>();

        public void Initialize(int samples)
        {
            XCutoffIndex = -1;
            YCutoffIndex = -1;

            Length = samples;
            CanFilter = Length > 10;

            Times = new long[samples];
            RawXs = new double[samples];
            RawYs = new double[samples];

            RawTotalDistance = new double[samples];
            RawSpeed = new double[samples];
            RawVerticalVelocity = new double[samples];
            RawHorizontalVelocity = new double[samples];
            RawAcceleration = new double[samples];
            RawVerticalAcceleration = new double[samples];
            RawHorizontalAcceleration = new double[samples];
            
            TotalDistance = new double[samples];
            Speed = new double[samples];
            VerticalVelocity = new double[samples];
            HorizontalVelocity = new double[samples];
            Acceleration = new double[samples];
            VerticalAcceleration = new double[samples];
            HorizontalAcceleration = new double[samples];
            
            AbsoluteAngle = new double[samples];
            DisplacementAngle = new double[samples];
            TotalDisplacementAngle = new double[samples];
            AngularVelocity = new double[samples];
            TangentialVelocity = new double[samples];
            CentripetalAcceleration = new double[samples];
            AngularAcceleration = new double[samples];

            components.Add("xRaw", RawXs);
            components.Add("x", Xs);
            components.Add("yRaw", RawYs);
            components.Add("y", Ys);
            components.Add("totalDistanceRaw", RawTotalDistance);
            components.Add("totalDistance", TotalDistance);

            components.Add("speedRaw", RawSpeed);
            components.Add("speed", Speed);
            components.Add("verticalVelocityRaw", RawVerticalVelocity);
            components.Add("verticalVelocity", VerticalVelocity);
            components.Add("horizontalVelocityRaw", RawHorizontalVelocity);
            components.Add("horizontalVelocity", HorizontalVelocity);

            components.Add("accelerationRaw", RawAcceleration);
            components.Add("acceleration", Acceleration);
            components.Add("verticalAccelerationRaw", RawVerticalAcceleration);
            components.Add("verticalAcceleration", VerticalAcceleration);
            components.Add("horizontalAccelerationRaw", RawHorizontalAcceleration);
            components.Add("horizontalAcceleration", HorizontalAcceleration);
            
            components.Add("absoluteAngle", AbsoluteAngle);
            components.Add("displacementAngle", DisplacementAngle);
            components.Add("totalDisplacementAngle", TotalDisplacementAngle);
            components.Add("angularVelocity", AngularVelocity);
            components.Add("tangentialVelocity", TangentialVelocity);
            components.Add("centripetalAcceleration", CentripetalAcceleration);
            components.Add("angularAcceleration", AngularAcceleration);
        }

        public PointF RawCoordinates(int index)
        {
            return new PointF((float)RawXs[index], (float)RawYs[index]);
        }

        public PointF Coordinates(int index)
        {
            if (XCutoffIndex < 0 || YCutoffIndex < 0)
                return RawCoordinates(index);
            else
                return new PointF((float)FilterResultXs[XCutoffIndex].Data[index], (float)FilterResultYs[YCutoffIndex].Data[index]);
        }

        public void ForceRawSeries()
        {
            TotalDistance = RawTotalDistance;
            Speed = RawSpeed;
            HorizontalVelocity = RawHorizontalVelocity;
            VerticalVelocity = RawVerticalVelocity;
            Acceleration = RawAcceleration;
            HorizontalAcceleration = RawVerticalAcceleration;
            VerticalAcceleration = RawVerticalAcceleration;
        }
    }
}
