﻿using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(RectVisualizer), Target = typeof(Rect))]
[assembly: TypeVisualizer(typeof(RectVisualizer), Target = typeof(Rect[]))]

namespace Bonsai.Vision.Design
{
    public class RectVisualizer : IplImageVisualizer
    {
        const float DefaultHeight = 480;
        const int DefaultThickness = 2;
        ObjectTextVisualizer textVisualizer;
        IDisposable inputHandle;
        IplImage input;
        IplImage canvas;

        static void Draw(IplImage image, Rect rect)
        {
            var color = image.Channels == 1 ? Scalar.Real(255) : Scalar.Rgb(255, 0, 0);
            var thickness = DefaultThickness * (int)Math.Ceiling(image.Height / DefaultHeight);
            CV.Rectangle(image, rect, color, thickness);
        }

        internal static void Draw(IplImage image, object value)
        {
            if (image != null)
            {
                var array = value as Rect[];
                if (array != null)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        Draw(image, array[i]);
                    }
                }
                else Draw(image, (Rect)value);
            }
        }

        public override void Show(object value)
        {
            if (textVisualizer != null) textVisualizer.Show(value);
            else
            {
                if (input != null)
                {
                    canvas = IplImageHelper.EnsureColorCopy(canvas, input);
                    Draw(canvas, value);
                    base.Show(canvas);
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            var imageInput = VisualizerHelper.ImageInput(provider);
            if (imageInput != null)
            {
                inputHandle = imageInput.Subscribe(value => input = (IplImage)value);
                base.Load(provider);
            }
            else
            {
                textVisualizer = new ObjectTextVisualizer();
                textVisualizer.Load(provider);
            }
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            if (textVisualizer != null) return textVisualizer.Visualize(source, provider);
            else return base.Visualize(source, provider);
        }

        public override void Unload()
        {
            if (canvas != null)
            {
                canvas.Close();
                canvas = null;
            }

            if (inputHandle != null)
            {
                inputHandle.Dispose();
                inputHandle = null;
            }

            if (textVisualizer != null)
            {
                textVisualizer.Unload();
                textVisualizer = null;
            }
            else base.Unload();
        }
    }
}
