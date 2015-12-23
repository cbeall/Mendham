﻿using Mendham.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mendham.Domain.Ninject.Test.TestObjects
{
    public sealed class Test1DomainEventHandler : BaseDomainEventHandler<Test1DomainEvent>
    {
        public override Task HandleAsync(Test1DomainEvent domainEvent)
        {
            return Task.FromResult(0);
        }
    }

    public sealed class Test2DomainEventHandler : BaseDomainEventHandler<Test2DomainEvent>
    {
        public override Task HandleAsync(Test2DomainEvent domainEvent)
        {
            return Task.FromResult(0);
        }
    }

    public sealed class Test1DomainEvent : DomainEvent
    { }

    public sealed class Test2DomainEvent : DomainEvent
    { }
}
