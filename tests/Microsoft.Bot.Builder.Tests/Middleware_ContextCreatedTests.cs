﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    [TestCategory("Russian Doll Middleware, Context Created Pipeline Tests")]
    public class Middleware_ContextCreatedTests
    {
        [TestMethod]
        public async Task NoMiddleware()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            await m.ReceiveActivity(null); // No middleware. Should not explode. 
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ThrowOnNullMiddlware()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use((Middleware.IMiddleware)null);            
        }

        [TestMethod]
        public async Task OneMiddlewareItem()
        {
            WasCalledMiddlware simple = new WasCalledMiddlware();

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(simple);

            Assert.IsFalse(simple.Called);
            await m.ContextCreated(null);
            Assert.IsTrue(simple.Called);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BubbleUncaughtException()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                throw new InvalidOperationException("test");
            }));

            await m.ContextCreated(null);
            Assert.Fail("Should never have gotten here");
        }

        [TestMethod]
        public async Task TwoMiddlewareItems()
        {
            WasCalledMiddlware one = new WasCalledMiddlware();
            WasCalledMiddlware two = new WasCalledMiddlware();

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.ContextCreated(null);
            Assert.IsTrue(one.Called);
            Assert.IsTrue(two.Called);
        }

        [TestMethod]
        public async Task TwoMiddlewareItemsInOrder()
        {
            bool called1 = false;
            bool called2 = false;

            CallMeMiddlware one = new CallMeMiddlware(() =>
            {
                Assert.IsFalse(called2, "Second Middleware was called");
                called1 = true;
            });

            CallMeMiddlware two = new CallMeMiddlware(() =>
            {
                Assert.IsTrue(called1, "First Middleware was not called");
                called2 = true;
            });

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(one);
            m.Use(two);

            await m.ContextCreated(null);
            Assert.IsTrue(called1);
            Assert.IsTrue(called2);
        }

        [TestMethod]
        public async Task AnonymousMiddleware()
        {
            bool didRun = false;

            MiddlewareSet m = new MiddlewareSet();
            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                didRun = true;
                await next();
            }));

            Assert.IsFalse(didRun);
            await m.ContextCreated(null);
            Assert.IsTrue(didRun);
        }

        [TestMethod]
        public async Task TwoAnonymousMiddleware()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                didRun1 = true;
                await next();
            }));

            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                didRun2 = true;
                await next();
            }));

            await m.ContextCreated(null);
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task TwoAnonymousMiddlewareInOrder()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            MiddlewareSet m = new Middleware.MiddlewareSet();
            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                Assert.IsFalse(didRun2, "Looks like the 2nd one has already run");
                didRun1 = true;
                await next();
            }));
            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                Assert.IsTrue(didRun1, "Looks like the 1nd one has not yet run");
                didRun2 = true;
                await next();
            }));

            await m.ContextCreated(null);
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task MixedMiddlewareInOrderAnonymousFirst()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();

            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                Assert.IsFalse(didRun2, "Looks like the 2nd one has already run");
                didRun1 = true;
                await next();
            }));

            m.Use(
                new CallMeMiddlware(() =>
                {
                    Assert.IsFalse(didRun2, "Second Middleware was called");
                    didRun2 = true;
                }));

            await m.ContextCreated(null);
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task MixedMiddlewareInOrderAnonymousLast()
        {
            bool didRun1 = false;
            bool didRun2 = false;

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();

            m.Use(
                new CallMeMiddlware(() =>
                {
                    Assert.IsFalse(didRun2, "Second Middleware was called");
                    didRun1 = true;
                }));

            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                Assert.IsTrue(didRun1, "Looks like the 1st middleware has not been run");
                didRun2 = true;
                await next();
            }));

            await m.ContextCreated(null);
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
        }

        [TestMethod]
        public async Task RunCodeBeforeAndAfter()
        {
            bool didRun1 = false;
            bool codeafter2run = false;
            bool didRun2 = false;

            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();

            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                Assert.IsFalse(didRun1, "Looks like the 1st middleware has already run");
                didRun1 = true;
                await next();
                Assert.IsTrue(didRun1, "The 2nd middleware should have run now.");
                codeafter2run = true;
            }));

            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                Assert.IsTrue(didRun1, "Looks like the 1st middleware has not been run");
                Assert.IsFalse(codeafter2run, "The code that runs after middleware 2 is complete has already run.");
                didRun2 = true;
                await next();
            }));

            await m.ContextCreated(null);
            Assert.IsTrue(didRun1);
            Assert.IsTrue(didRun2);
            Assert.IsTrue(codeafter2run);
        }

        [TestMethod]
        public async Task CatchAnExceptionViaMiddlware()
        {
            Middleware.MiddlewareSet m = new Middleware.MiddlewareSet();
            bool caughtException = false;

            m.Use( new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                try
                {
                    await next();
                    Assert.Fail("Should not get here");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message == "test");
                    caughtException = true;
                }
            }));

            m.Use(new AnonymousContextCreatedMiddleware(async (context, next) =>
            {
                throw new Exception("test");
            }));

            await m.ContextCreated(null);
            Assert.IsTrue(caughtException);
        }

        public class WasCalledMiddlware : Middleware.IMiddleware, Middleware.IContextCreated
        {
            public bool Called { get; set; } = false;

            public Task ContextCreated(IBotContext context, Middleware.MiddlewareSet.NextDelegate next)
            {
                Called = true;
                return next();
            }
        }

        public class CallMeMiddlware : Middleware.IMiddleware, Middleware.IContextCreated
        {
            private readonly Action _callMe;
            public CallMeMiddlware(Action callMe)
            {
                _callMe = callMe;
            }
            public Task ContextCreated(IBotContext context, Middleware.MiddlewareSet.NextDelegate next)
            {
                _callMe();
                return next();
            }
        }
    }
}
