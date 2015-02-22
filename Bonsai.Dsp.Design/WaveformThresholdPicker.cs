﻿using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Dsp.Design
{
    class WaveformThresholdPicker : WaveformView
    {
        static readonly TimeSpan PickerRefreshInterval = TimeSpan.FromMilliseconds(30);
        IDisposable thresholdNotifications;
        bool initialized;

        public WaveformThresholdPicker()
        {
            Chart.IsEnableZoom = false;
            InitializeReactiveEvents();
        }

        public double[] Threshold { get; set; }

        public event EventHandler ThresholdChanged;

        protected virtual void OnThresholdChanged(EventArgs e)
        {
            var handler = ThresholdChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void InitializeReactiveEvents()
        {
            var scheduler = new ControlScheduler(this);
            var thresholdDrag = (from mouseDown in Chart.MouseDown
                                 where mouseDown.Button == MouseButtons.Left
                                 let pane = Chart.MasterPane.FindChartRect(mouseDown.Location)
                                 where pane != null
                                 let firstThreshold = GetLocationValue(pane, mouseDown.Location)
                                 select Observable.Return(firstThreshold).Concat(
                                        (from mouseMove in Chart.MouseMove.TakeUntil(Chart.MouseUp)
                                                                    .Sample(PickerRefreshInterval, scheduler)
                                         select GetLocationValue(pane, mouseMove.Location)))
                                         .Select(threshold => new { pane, threshold }))
                                         .Merge();
            thresholdNotifications = thresholdDrag.Subscribe(xs => ProcessThreshold(xs.pane, xs.threshold));
        }

        private double GetLocationValue(GraphPane pane, PointF point)
        {
            double x, y;
            pane.ReverseTransform(point, out x, out y);
            return y;
        }

        private void ProcessThreshold(GraphPane pane, double threshold)
        {
            var channelIndex = (int)pane.Tag;
            var thresholdValues = EnsureThreshold();
            thresholdValues[channelIndex] = threshold;

            SetThresholdLine(pane, pane.XAxis.Scale.Min, pane.XAxis.Scale.Max, threshold);
            OnThresholdChanged(EventArgs.Empty);
        }

        private void SetThresholdLine(GraphPane pane, double x0, double x1, double threshold)
        {
            var thresholdLine = pane.GraphObjList.FirstOrDefault();
            if (thresholdLine == null)
            {
                thresholdLine = new LineObj(Color.Red, x0, threshold, x1, threshold);
                pane.GraphObjList.Add(thresholdLine);
            }
            else thresholdLine.Location.Y = threshold;
        }

        private double[] EnsureThreshold()
        {
            var channelCount = Chart.ChannelCount;
            var thresholdValues = Threshold ?? new double[channelCount];
            if (thresholdValues.Length != channelCount) Array.Resize(ref thresholdValues, channelCount);
            Threshold = thresholdValues;
            return thresholdValues;
        }

        public override void EnsureWaveform(int rows, int columns)
        {
            base.EnsureWaveform(rows, columns);
            if (!initialized)
            {
                initialized = true;
                var thresholdValues = EnsureThreshold();
                foreach (var pane in Chart.MasterPane.PaneList)
                {
                    var channelIndex = (int)pane.Tag;
                    var threshold = thresholdValues[channelIndex];
                    SetThresholdLine(pane, 0, columns, threshold);
                }
            }
        }

        protected override void OnSelectedPageChanged(EventArgs e)
        {
            initialized = false;
            base.OnSelectedPageChanged(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (thresholdNotifications != null)
            {
                thresholdNotifications.Dispose();
                thresholdNotifications = null;
            }
            base.OnHandleDestroyed(e);
        }
    }
}
