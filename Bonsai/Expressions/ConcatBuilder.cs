﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Concat", Namespace = Constants.XmlNamespace)]
    [Description("Ensures that values of the second sequence are propagated only after the first sequence terminates.")]
    public class ConcatBuilder : BinaryCombinatorExpressionBuilder
    {
        static readonly MethodInfo concatMethod = typeof(Observable).GetMethods().Single(m => m.Name == "Concat" &&
                                                                                         m.GetParameters().Length == 2);

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            return Expression.Call(concatMethod.MakeGenericMethod(sourceType, otherType), Source, Other);
        }
    }
}
