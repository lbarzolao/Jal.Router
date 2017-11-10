﻿using System;
using Jal.Router.Interface.Inbound;
using Jal.Router.Model;

namespace Jal.Router.Impl.Inbound
{
    public abstract class AbstractRouterInterceptor : IRouterInterceptor
    {
        public virtual void OnEntry(MessageContext context)
        {

        }

        public virtual void OnSuccess<TContent>(MessageContext context, TContent content)
        {

        }

        public virtual void OnExit(MessageContext context)
        {

        }

        public virtual void OnException(MessageContext context, Exception exception)
        {

        }
    }
}