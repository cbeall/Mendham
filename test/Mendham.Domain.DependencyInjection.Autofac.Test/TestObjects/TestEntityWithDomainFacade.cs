﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mendham.Domain.Events;

namespace Mendham.Domain.DependencyInjection.Autofac.Test.TestObjects
{
	public class TestEntityWithDomainFacade : Entity
	{
		public delegate TestEntityWithDomainFacade Factory(int id);

		private readonly IFacade domainFacade;

		public TestEntityWithDomainFacade(int id, IFacade facade)
		{
			this.Id = id;

			this.domainFacade = facade;
		}

		public int Id { get; private set; }

		public bool HasValidFacade()
		{
			return domainFacade != null && domainFacade.HasDomainEventPublisher();
		}

		public interface IFacade : IDomainFacade
		{
			bool HasDomainEventPublisher();
		}

		public class Facade : DomainFacade, IFacade
		{
			private readonly bool domainEventPublisherHasValue;

			public Facade(IDomainEventPublisher domainEventPublisher)
				: base(domainEventPublisher)
			{
				domainEventPublisherHasValue = domainEventPublisher != null;
			}

			public bool HasDomainEventPublisher()
			{
				return domainEventPublisherHasValue;
			}
		}

		protected override IEnumerable<object> IdentityComponents
        {
			get
			{
				yield return Id;
			}
		}
	}
}