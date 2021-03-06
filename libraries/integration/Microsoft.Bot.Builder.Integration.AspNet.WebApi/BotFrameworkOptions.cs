﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Connector.Authentication;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Integration.AspNet.WebApi
{
    public class BotFrameworkOptions
    {
        private readonly List<IMiddleware> _middleware;
        private readonly BotFrameworkPaths _paths;

        public BotFrameworkOptions()
        {
            _middleware = new List<IMiddleware>();
            _paths = new BotFrameworkPaths();
        }

        public ICredentialProvider CredentialProvider { get; set; }
        public List<IMiddleware> Middleware { get => _middleware; }
        public bool EnableProactiveMessages { get; set; }
        public BotFrameworkPaths Paths { get => _paths; }
    }
}