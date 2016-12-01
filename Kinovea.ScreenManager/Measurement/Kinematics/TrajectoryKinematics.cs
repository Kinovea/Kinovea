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
        public double[] TotalDistance { get; set; }

        public double[] RawSpeed { get; private set; }
        public double[] Speed { get; set; }
        public double[] RawVerticalVelocity { get; private set; }
        public double[] VerticalVelocity { get; set; }
        public double[] RawHorizontalVelocity { get; private set; }
        public double[] HorizontalVelocity { get; set; }

        public double[] RawAcceleration { get; private set; }
        public double[] Acceleration { get; set; }
        public double[] RawVerticalAcceleration { get; private set; }
        public double[] VerticalAcceleration { get; set; }
        public double[] RawHorizontalAcceleration { get; private set; }
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
            TotalDistance = new double[samples];

            RawSpeed = new double[samples];
            Speed = new double[samples];
            RawVerticalVelocity = new double[samples];
            VerticalVelocity = new double[samples];
            RawHorizontalVelocity = new double[samples];
            HorizontalVelocity = new double[samples];

            RawAcceleration = new double[samples];
            Acceleration = new double[samples];
            RawVerticalAcceleration = new double[samples];
            VerticalAcceleration = new double[samples];
            RawHorizontalAcceleration = new double[samples];
            HorizontalAcceleration = new double[samples];
            
            AbsoluteAngle = new double[samples];
            DisplacementAngle = new double[samples];
            TotalDisplacementAngle = new double[samples];
            AngularVelocity = new double[samples];
            TangentialVelocity = new double[samples];
            CentripetalAcceleration = new double[samples];
            AngularAcceleration = new double[samples];
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
