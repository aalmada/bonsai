﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Creates a rotation matrix from a quaternion representation.")]
    public class CreateQuaternionRotation : Source<Matrix4>
    {
        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        [Description("The quaternion representing the rotation transformation.")]
        public Quaternion Rotation { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateFromQuaternion(Rotation)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.CreateFromQuaternion(Rotation));
        }
    }
}