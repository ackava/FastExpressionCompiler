﻿using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace FastExpressionCompiler.UnitTests
{
    [TestFixture]
    public class PreConstructedClosureTests
    {
        [Test]
        public void Can_pass_closure_with_constant_to_TryCompile()
        {
            var x = new X();
            var xConstExpr = Expression.Constant(x);
            var expr = Expression.Lambda<Func<X>>(xConstExpr);

            var c = ExpressionCompiler.Closure.Create(x);
            var f = expr.TryCompileWithPreCreatedClosure<Func<X>>(c, xConstExpr);

            Assert.IsInstanceOf<X>(f());
        }

        public class X { }

        [Test]
        public void Can_pass_ANY_class_closure_with_constant_to_TryCompile()
        {
            var x = new X();
            var xConstExpr = Expression.Constant(x);
            var expr = Expression.Lambda<Func<X>>(xConstExpr);

            var cx = new ClosureX(x);
            var f = expr.TryCompileWithPreCreatedClosure<Func<X>>(cx, xConstExpr);

            Assert.IsInstanceOf<X>(f());
        }

        public class ClosureX
        {
            public readonly X X;
            public ClosureX(X x) { X = x; }
        }

        [Test]
        public void Can_prevent_closure_creation_when_compiling_a_static_delegate()
        {
            Expression<Func<X>> expr = () => new X();

            var f = expr.TryCompileWithPreCreatedClosure<Func<X>>(closure: null);

            Assert.IsInstanceOf<X>(f());
        }

        [Test]
        public void Can_pass_closure_to_hoisted_expr_with_nested_lambda()
        {
            var x = new X();
            Expression<Func<Y>> expr = () => new Y(x, () => x);

            var f1 = expr.TryCompile<Func<Y>>();
            var y = f1();

            Assert.IsInstanceOf<Y>(y);
            Assert.AreSame(y.A, y.B);
        }

        public class Y
        {
            public readonly X A;
            public readonly X B;

            public Y(X a, X b)
            {
                A = a;
                B = b;
            }

            public Y(X a, Func<X> fb)
            {
                A = a;
                B = fb();
            }
        }
    }
}
