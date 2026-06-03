using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.GUI.MRU;
using Rhino.Geometry;
using Woodpecker.Animation.Geometry.Display;
using Woodpecker.Animation.Util.IO;


namespace Woodpecker.Animation.Control.Timeline
{
    /// <summary>
    /// A class that represents a sequential timeline. It has a list of time points and a flip state.
    /// The GetTimePoint method returns the list of time points based on the flip state and the inverse result. 
    /// The flip state is determined by the first and last time points, and the inverse result is toggled when the flip state changes. 
    /// The time points are reversed when the flip state is true. The GetTimePoint method also updates the flip state and the 
    /// inverse result based on the current time points.
    /// </summary>
    public class SequetialTimeline
    {
        bool _flip = false;
        bool _InverseResult = false;
        List<double> _timePoint = new List<double>();
        private SequetialTimeline() { }
        public SequetialTimeline(List<double> timePoint)
        {
            _timePoint = timePoint;
        }
        public List<double> GetTimePoint(ref bool state)
        {
            if (_timePoint == null) return new List<double>();
            var FirR = _InverseResult ? false : true;
            var SecR = _InverseResult ? true : false;

            if (_timePoint.Last() >= 1 - 1e-2)
            {
                _flip = FirR;
            }
            else if (_timePoint[0] <= 1e-2)
            {
                _flip = SecR;
            }
            if (_flip)
                _InverseResult = !_InverseResult;

            var outList = new List<double>(_timePoint);
            if (_flip)
                outList.Reverse();

            _timePoint = outList;

            state = _flip;
            return outList;
        }
    }
    /// <summary>
    /// A static class that provides settings and utility methods for the timeline control. It includes a version property, an easing function delegate, a default easing function, an easing method, an interval range method, a timeline delay method, an activative timeline method, and segmentise timeslot methods. The easing function delegate defines the signature for custom easing functions that can be used in the timeline control. The default easing function is a smooth step function that eases in and out. The easing method calculates the eased percentage based on the input percentage of the timeline and the specified easing function. The interval range method calculates the minimum and maximum values of a list of intervals. The timeline delay method calculates a new time zone by adding delay and prolong to the original time zone. The activative timeline method calculates the current percentage of the timeline based on the input time and the in and out time zones. The segmentise timeslot methods segmentise the timeline into several segments with linear or non-linear overlapping based on the input parameters.
    /// </summary>
    public static class TimelineSetting
    {
        public static string Version => "1.0.0";
        /// <summary>
        /// Create a timeline interval based on the input start time and period. If the period is positive, the interval is from start to start + period. If the period is negative, the interval is from start + period to start. The interval represents the time zone of the timeline, where the minimum value is the starting time and the maximum value is the ending time. The length of the interval is equal to the absolute value of the period. This method allows you to create a timeline with a specified duration and direction based on the input parameters.
        /// </summary>
        /// <param name="Start">The start time of the timeline interval.</param>
        /// <param name="Period">The period of the timeline interval.</param>
        /// <returns>The created timeline interval.</returns>
        public static Interval CreateTimeline(double Start, double Period)
        => Period > 0 ? new Interval(Start, Start + Period) : new Interval(Start + Period, Start);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Start">The start timeslot</param>
        /// <param name="AccumulatedTime"></param>
        /// <param name="Shift"></param>
        /// <param name="Speed"></param>
        /// <returns></returns>
        public static List<Interval> CreateTimelineByAccumulatedTime(double Start, List<double> AccumulatedTime, List<double> Shift, double Speed)
        {
            if (AccumulatedTime == null) return null;
            if (Shift == null)
            {
                Shift = Enumerable.Repeat(0.0, AccumulatedTime.Count).ToList();
            }
            if (Shift.Count < AccumulatedTime.Count)
            {
                for (int i = Shift.Count - 1; i < AccumulatedTime.Count; i++)
                {
                    Shift.Add(0);
                }
            }
            Speed = Speed <= 0 ? 1 : Speed;
            Speed = 1 / Speed;
            var iTimeLine = new List<Interval>();
            var Pointer = Start;
            for (int i = 0; i < AccumulatedTime.Count; i++)
            {
                Pointer += Shift[i] * Speed;
                iTimeLine.Add(new Interval(Pointer, Pointer + AccumulatedTime[i] * Speed));
                Pointer += AccumulatedTime[i] * Speed;
            }
            return iTimeLine;
        }
        /// <summary>
        /// Easing function. The input t is the percentage of the timeline, and the output is the eased percentage. The values parameter can be used to pass additional parameters to the easing function. The default easing function is a smooth step function that eases in and out. You can define your own easing function by implementing the EasingFunction delegate and passing it to the Easing method.
        /// </summary>
        /// <param name="t">The input percentage of the timeline. t = [0,1]</param>
        /// <param name="values">The additional parameters for the easing function.</param>
        /// <returns>The eased percentage.</returns>
        public delegate double EasingFunction(double t, params double[] Values);
        /// <summary>
        /// Default easing function. It is a smooth step function that eases in and out. The input t is the percentage of the timeline, and the output is the eased percentage. The values parameter is not used in this function. t = t * t * (3 - 2 * t) is a common formula for a smooth step function that eases in and out. It starts at 0 when t = 0, and ends at 1 when t = 1. The curve is smooth and has a gentle slope at the beginning and end, which creates a natural easing effect.
        /// </summary>
        /// <param name="t">The input percentage of the timeline. t = [0,1]</param>
        /// <param name="Values">The additional parameters for the easing function.</param>
        /// <returns>The eased percentage.</returns>
        private static double DefaultEasing(double t, params double[] Values)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));
            t = t * t * (3 - 2 * t);
            return t;
        }
        /// <summary>
        /// Easing method. It takes the input percentage of the timeline, the easing function, and the additional parameters for the easing function. It returns the eased percentage. If the easing function is not provided, it uses the default easing function. The input t is clamped between 0 and 1 before being passed to the easing function. The output is the eased percentage based on the input t and the easing function.
        /// </summary>
        /// <param name="t">The input percentage of the timeline. t = [0,1]</param>
        /// <param name="Function">The easing function.</param>
        /// <param name="Values">The additional parameters for the easing function.</param>
        /// <returns>The eased percentage.</returns>
        public static double Easing(double t, EasingFunction Function, params double[] Values)
        => Function(t, Values);
        /// <summary>
        /// Easing method with default easing function. It takes the input percentage of the timeline and returns the eased percentage using the default easing function. The input t is clamped between 0 and 1 before being passed to the default easing function. The output is the eased percentage based on the input t and the default easing function.
        /// </summary>
        /// <param name="t">The input percentage of the timeline. t = [0,1]</param>
        /// <returns>The eased percentage.</returns>
        public static double Easing(double t)
        => Easing(t, DefaultEasing);
        /// <summary>
        /// Calculate the range of a list of intervals. If the list is empty, return [0,1].
        /// </summary>
        /// <param name="intervals">The list of intervals.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        public static void IntervalRange(List<Interval> Tintervals, ref double min, ref double max)
        {
            if (Tintervals == null || Tintervals.Count == 0)
            {
                min = 0;
                max = 1;
                return;
            }
            var minBound = 1000000.0;
            var maxBound = 0.0;
            foreach (var In in Tintervals)
            {
                if (minBound > In.Min)
                    minBound = In.Min;
                if (maxBound < In.Max)
                    maxBound = In.Max;
            }
            min = minBound;
            max = maxBound;
        }
        /// <summary>
        /// Calculate the new time zone by adding delay and prolong to the original time zone. If the delay is null, return null. If the original time zone is null, return null. If the prolong is null, set it to a list of 0 with the same length as the delay. If the prolong has less elements than the delay, repeat the last element until it has the same length as the delay.
        /// </summary>
        /// <param name="Tinterval">The original time zone.</param>
        /// <param name="delay">The list of delays.</param>
        /// <param name="Prolong">The list of prolongs.</param>
        /// <returns>The new time zone.</returns>
        public static List<Interval> TimelineDelay(Interval Tinterval, List<double> delay, List<double> Prolong, double Speed = 1)
        {
            if (delay == null) return null;
            if (Tinterval == Interval.Unset) return null;
            if (Prolong == null) Prolong = Enumerable.Repeat(0.0, delay.Count).ToList();
            else
                DataUtil.AlignList(ref delay, ref Prolong);
            var NewTimeZone = new List<Interval>();
            for (int i = 0; i < delay.Count; i++)
            {
                var x = delay[i];
                NewTimeZone.Add(new Interval(Tinterval.Min + x, Tinterval.Max + x + Math.Round(Prolong[i] / Speed, 4)));
            }

            return NewTimeZone;
        }
        /// <summary>
        /// Calculate the current percentage of the timeline based on the input time and the in and out time zones. If the in time zone is [0,0] and the out time zone is invalid, return 0. If the input time is less than or equal to the minimum of the in time zone, return 0. If the input time is between the minimum and maximum of the in time zone, return the percentage of the input time in the in time zone. If the input time is greater than or equal to the maximum of the in time zone and less than or equal to the minimum of the out time zone, return 1. If the input time is greater than or equal to the maximum of the out time zone, return 0. Otherwise, return 1 minus the percentage of the input time in the out time zone.
        /// </summary>
        /// <param name="Tin">The in time zone.</param>
        /// <param name="Tout">The out time zone.</param>
        /// <param name="T">The input time. The global time T = [a, b]</param>
        /// <returns>The current percentage of the timeline. t = [0,1] </returns>
        public static double ActivativeTimeline(Interval TLin, Interval TLout, double G_T, int round = 4)
        {
            double CurrentP = 0;
            var Tin = RoundInterval(TLin, round);
            var Tout = RoundInterval(TLout, round);
            var T = Math.Round(G_T, round);
            if (Tin != new Interval(0, 0) || !Tout.IsValid) //Invalid OutTimeZone
            {
                if (T <= Tin.Min) CurrentP = 0;
                else if (Tin.IncludesParameter(T)) CurrentP = (T - Tin.Min) / Tin.Length;
                else if (T >= Tin.Max && T <= Tout.Min) CurrentP = 1;
                else if (T >= Tout.Max) CurrentP = 0;
                else CurrentP = 1 - ((T - Tout.Min) / Tout.Length);
            }
            else
            {
                if (T <= Tin.Min) CurrentP = 0;
                else if (Tin.IncludesParameter(T)) CurrentP = (T - Tin.Min) / Tin.Length;
                else CurrentP = 1;
            }
            return CurrentP;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Tin"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static double ActivativeTimeline(Interval TLin, double G_T, int round = 4)
        {
            double CurrentP = 0;
            var Tin = RoundInterval(TLin, round);
            var T = Math.Round(G_T, round);
            if (Tin != new Interval(0, 0))
            {
                if (T <= Tin.Min) CurrentP = 0;
                else if (Tin.IncludesParameter(T)) CurrentP = (T - Tin.Min) / Tin.Length;
                else if (T >= Tin.Max) CurrentP = 1;
            }
            return CurrentP;
        }
        
        
        public static double ActivativeCircularTimeline(Interval TLin, Interval TLout, double G_T, int round = 4)
        {
            if(TLin.IncludesParameter(G_T) || TLout.IncludesParameter(G_T))
            return ActivativeTimeline(TLin, TLout, G_T, round);
            else
            {
                var period = TLin.Length + TLout.Length + TLout.Min - TLin.Max;
                if(G_T > TLin.Max && G_T < TLout.Min)
                {
                    return 1;
                }
                if(G_T < TLin.Min)
                {
                    G_T += period;
                    return ActivativeCircularTimeline(TLin, TLout, G_T, round);
                }
                if(G_T > TLout.Max)
                {
                    G_T -= period;
                    return ActivativeCircularTimeline(TLin, TLout, G_T, round);
                }
                return -1;
            }
        }
        
        public static double ActivativeCircularTimeline(Interval TLin, double G_T, int round = 4)
        {
            if(TLin.IncludesParameter(G_T))
            {
                return ActivativeTimeline(TLin, G_T, round);
            }
            else
            {
                if(G_T < TLin.Min)
                {
                    G_T += TLin.Length;
                    return  ActivativeCircularTimeline(TLin, G_T, round);
                }
                if(G_T > TLin.Max)
                {
                    G_T -= TLin.Length;
                    return  ActivativeCircularTimeline(TLin, G_T, round);
                }
                return -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="ParameterRange"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static double RemapTtoSliderControl(double t, Interval ParameterRange, int digits)
        {
            t = Math.Max(0, Math.Min(t, 1));
            var newvalue = ParameterRange.Min + ParameterRange.Length * t;
            return Math.Round(newvalue, digits);
        }
        /// <summary>
        /// If ActivatedInZone is true, calculate the current percentage of the timeline based on the input time and the in and out time zones. 
        /// If the in time zone is [0,0] and the out time zone is invalid, return 0. If the input time is less than or equal to the minimum of the in time zone, 
        /// return 0. If the input time is between the minimum and maximum of the in time zone, return the percentage of the input time in the in time zone. If the input time is greater than or equal to the maximum of the in time zone and less than or equal to the minimum of the out time zone, return 1. If the input time is greater than or equal to the maximum of the out time zone, return 0. Otherwise, return 1 minus the percentage of the input time in the out time zone.
        /// If ActivatedInZone is false, return -1 if the input time is between the minimum and maximum of the in time zone and not between the minimum and maximum of the out time zone. Otherwise, calculate the current percentage of the timeline based on the input time and the in and out time zones as described above.
        /// </summary>
        /// <param name="ActivedInZone"> Activative the timeline</param>
        /// <param name="Tin"> The in time zone</param>
        /// <param name="Tout"> The out time zone</param>
        /// <param name="T"> The input time. The global time T = [a, b]</param>
        /// <returns> The current percentage of the timeline, t = [0,1]</returns>
        public static double ActivativeTimeline(bool ActivedInZone, Interval TLin, Interval TLout, double G_T, int round = 3)
        {
            double CurrentP = 0;
            var Tin = RoundInterval(TLin, round);
            var Tout = RoundInterval(TLout, round);
            var T = Math.Round(G_T, round);
            if (ActivedInZone &&
        !Tin.IncludesParameter(T) &&
        (!Tout.IncludesParameter(T) || Tout == new Interval(0, 0))
        )
            {
                CurrentP = -1;
                return CurrentP;
            }

            // Write your logic here
            if (Tout != new Interval(0, 0) || !Tout.IsValid) //Invalid OutTimeZone
            {
                if (T <= Tin.Min) CurrentP = 0;
                else if (Tin.IncludesParameter(T)) CurrentP = (T - Tin.Min) / Tin.Length;
                else if (T >= Tin.Max && T <= Tout.Min)
                {
                    CurrentP = 1;
                }
                else if (T >= Tout.Max) CurrentP = 0;
                else CurrentP = 1 - ((T - Tout.Min) / Tout.Length);
            }
            else
            {
                if (T <= Tin.Min) CurrentP = 0;
                else if (Tin.IncludesParameter(T)) CurrentP = (T - Tin.Min) / Tin.Length;
                else CurrentP = 1;
            }
            return CurrentP;
        }
        public static double ActivativeTimeline(bool ActivedInZone, Interval TLin, double G_T, int round = 3)
        {
            var Tin = RoundInterval(TLin, round);
            var T = Math.Round(G_T, round);
            if (ActivedInZone && !Tin.IncludesParameter(T))
            {
                return -1;
            }
            else
            {
                // Write your logic here
                return ActivativeTimeline(Tin, T);
            }
        }
        /// <summary>
        /// Segmentise the timeline into several segments with linear overlapping. The input t is the percentage of the timeline, Segs is the number of segments, Overlap is the percentage of overlapping between segments, ts is the list of percentages for each segment, and tlen is the length of each segment. The output ts is a list of percentages for each segment based on the input t and the overlapping. The output tlen is the length of each segment based on the number of segments and the overlapping. If Segs is less than or equal to 1, return a list with only one element t. If the calculated length of each segment is less than or equal to a small threshold (1e-12), return a list of zeros for ts and set tlen to 0. Otherwise, calculate the percentages for each segment based on the input t and the overlapping, and return them in ts along with the calculated tlen.
        /// </summary>
        /// <param name="t"> The percentage of the timeline. t = [0,1]</param>
        /// <param name="Segs"> The number of segments. Segs = [1,∞)</param>
        /// <param name="Overlap"> The percentage of overlapping between segments. Overlap = [0,1)</param>
        /// <param name="ts"> The list of percentages for each segment. ts = {x1, x2, ..., xn | xi ∈ [0,1]}</param>
        /// <param name="tlen"> The length of each segment. tlen = [0,1]</param>
        public static void SegmentiseTimeslotLinear(double t, int Segs, double Overlap, ref List<double> ts, ref double tlen)
        {
            if (Segs <= 1)
            {
                ts = new List<double> { t };
            }
            var teLen = (1 + Overlap * (Segs - 1)) / Segs;

            if (teLen <= 1e-12)
            {
                ts = Enumerable.Repeat(0.0, Segs).ToList();
                return;
            }

            var teList = new List<double>();

            for (int i = 0; i < Segs; i++)
            {
                double Val = (t - (teLen - Overlap) * i) / teLen;
                teList.Add(Val <= 0 ? 0 : Val >= 1 ? 1 : Val);
            }
            ts = teList;
            tlen = teLen;
        }
        /// <summary>
        /// Segmentise the timeline into several segments with non-linear overlapping. The input t is the percentage of the timeline, Segs is the number of segments, Overlaps is a list of percentages of overlapping between segments, Function is the easing function to calculate the percentages for each segment, ts is the list of percentages for each segment, and tlen is the length of each segment. The output ts is a list of percentages for each segment based on the input t, the overlapping, and the easing function. The output tlen is the length of each segment based on the number of segments and the overlapping. If Segs is less than or equal to 1, return a list with only one element t. If the calculated length of each segment is less than or equal to a small threshold (1e-12), return a list of zeros for ts and set tlen to 0. Otherwise, calculate the percentages for each segment based on the input t, the overlapping, and the easing function, and return them in ts along with the calculated tlen.
        /// </summary>
        /// <param name="t"> The percentage of the timeline. t = [0,1]</param>
        /// <param name="Segs"> The number of segments. Segs = [1,∞)</param>
        /// <param name="Overlaps"> A list of percentages of overlapping between segments. Overlaps = {x1, x2, ..., xn | xi ∈ [0,1)}</param>
        /// <param name="ts"> The list of percentages for each segment. ts = {x1, x2, ..., xn | xi ∈ [0,1]}</param>
        /// <param name="tlen"> The length of each segment. tlen = [0,1]</param>
        /// <exception cref="Exception"></exception>
        public static void SegmentiseTimeslotNonLinear(double t, int Segs, List<double> Overlaps, ref List<double> ts, ref double tlen)
        {
            if (Overlaps == null) return;

            if (Segs <= 1)
            {
                ts = new List<double> { t };
            }
            if (Overlaps.Count != Segs - 1)
            {
                var TempOverlaps = new List<double>();
                for (int i = 0; i < Segs - 1; i++)
                {
                    var Ind = i > Overlaps.Count - 1 ? Overlaps.Count - 1 : i;
                    TempOverlaps.Add(Overlaps[Ind]);
                }
                Overlaps = TempOverlaps;
            }
            var Sai = Overlaps.Aggregate((x, y) => x + y);

            var XSlots = (1 + Sai) / Segs;
            ts = new List<double> { XSlots };

            if (XSlots <= 1e-12)
            {
                ts = new List<double> { 0 };
                throw new Exception("The time overlapped zone is too small");
            }

            var teList = new List<double>();
            var TimeSub = Overlaps.Select(x => XSlots - x).ToList();
            var Gather = 0.0;
            for (int i = 0; i < Segs; i++)
            {
                if (i > 0)
                    Gather += TimeSub[i - 1];

                var Val = (t - Gather) / XSlots;
                teList.Add(Val <= 0 ? 0 : Val >= 1 ? 1 : Val);

            }
            ts = teList;
            tlen = XSlots;
        }
        /// <summary>
        /// Get the easing pattern as a polyline curve based on the input easing function and its parameters. The input Function is the easing function to calculate the eased percentage for each point on the curve, and the Values are the additional parameters for the easing function. The output is a polyline curve that represents the easing pattern, where the x-coordinate is the input percentage of the timeline (from 0 to 1) and the y-coordinate is the eased percentage calculated by the easing function. The curve is sampled at intervals of 0.1 along the x-axis, resulting in 11 points on the curve (including both endpoints). Each point is created as a Point3d with z-coordinate set to 0, and then a PolylineCurve is constructed from these points to represent the easing pattern visually.
        /// </summary>
        /// <param name="Function">The easing function to calculate the eased percentage for each point on the curve.</param>
        /// <param name="Values">The additional parameters for the easing function.</param>
        /// <returns>A polyline curve representing the easing pattern.</returns>
        public static PolylineCurve GetEasingPattern(EasingFunction Function, params double[] Values)
        {
            var PtList = new List<Point3d>();
            for (double i = 0; i <= 1; i += 0.1)
            {
                PtList.Add(new Point3d(i, Function(i, Values), 0));
            }
            return new PolylineCurve(PtList);
        }
        public static string TimelineDescription(Interval Timeline, int round = 4)
        => $"{Math.Round(Timeline.Min, round)} - {Math.Round(Timeline.Max, round)}";
        public static Interval RoundInterval(Interval interval, int round = 4)
        => new Interval(Math.Round(interval.Min, round), Math.Round(interval.Max, round));
        public static Interval RedefindTimeline(Interval timeline, bool startorEnd, double period, double speed = 1)
        {
            speed = speed != 0 ? speed : 1;
            return startorEnd ?
            new Interval(timeline.Min, timeline.Min + period / speed) :
            new Interval(timeline.Max - period / speed, timeline.Max);
        }
    }
}