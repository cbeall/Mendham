﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Mendham.Domain.Events
{
	public class DomainEventHandlerContainer : IDomainEventHandlerContainer
	{
		private IEnumerable<IDomainEventHandler> domainEventHandlers;

		public DomainEventHandlerContainer(IEnumerable<IDomainEventHandler> domainEventHandlers)
		{
			this.domainEventHandlers = domainEventHandlers;
		}

		public async Task HandleAllAsync<TDomainEvent>(TDomainEvent domainEvent)
			where TDomainEvent : IDomainEvent
		{
			var handleTasks = domainEventHandlers
				.Where(HandlesDomainEvent<TDomainEvent>)
				.Select(GetGenericDomainEventHandlerForDomainEvent<TDomainEvent>)
				.Select(handler => HandleAsync(handler, domainEvent))
				.ToList();

			try
			{
				await Task.WhenAll(handleTasks);
			}
			catch (DomainEventHandlingException ex)
			{
				var dehExceptions = handleTasks
					.Where(a => a.Exception != null)
					.SelectMany(a => a.Exception.InnerExceptions)
					.OfType<DomainEventHandlingException>();

				if (dehExceptions.Count() > 1)
					throw new AggregateDomainEventHandlingException(dehExceptions, ex);

				throw ex;
            }
		}

		private static bool HandlesDomainEvent<TDomainEvent>(IDomainEventHandler handler)
		{
			var expectedDomainEventTypeInfo = typeof(TDomainEvent).GetTypeInfo();
			var handlerInterfaceDomainEventType = GetDomainEventTypeFromHandler(handler);

			if (handlerInterfaceDomainEventType == default(Type))
				return false;

			return handlerInterfaceDomainEventType
				.GetTypeInfo()
				.IsAssignableFrom(expectedDomainEventTypeInfo);
		}

		private static Type GetDomainEventTypeFromHandler(IDomainEventHandler handler)
		{
			var handlerInterface = handler
				.GetType()
				.GetInterfaces()
				.FirstOrDefault(IsGenericDomainEventHandler);

			if (handlerInterface == default(Type))
				return default(Type);

			return handlerInterface.GetGenericArguments()[0];
		}
		
		private static bool IsGenericDomainEventHandler(Type t)
		{
			var ti = t.GetTypeInfo();

			return ti.IsInterface
				&& ti.IsGenericType
				&& ti.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>);
		}

		private static IDomainEventHandler<TDomainEvent> GetGenericDomainEventHandlerForDomainEvent<TDomainEvent>(IDomainEventHandler handler)
			where TDomainEvent :  IDomainEvent
		{
			var match = handler as IDomainEventHandler<TDomainEvent>;

			if (match != null)
				return match;

			var baseDomainEventType = GetDomainEventTypeFromHandler(handler);

			var genericDomainEventHandlerWrapper = typeof(DomainEventHandlerWrapper<,>);
			var constructedDomainEventHandlerWrapper = genericDomainEventHandlerWrapper
				.MakeGenericType(baseDomainEventType, typeof(TDomainEvent));

			return (IDomainEventHandler<TDomainEvent>)
				Activator.CreateInstance(constructedDomainEventHandlerWrapper, handler);
		}

		private async Task HandleAsync<TDomainEvent>(IDomainEventHandler<TDomainEvent> handler, TDomainEvent domainEvent)
			where TDomainEvent :  IDomainEvent
		{
			try
			{
				await handler.HandleAsync(domainEvent);
			}
			catch (Exception ex)
			{
				Type handlerType = handler.GetType();

				var wrapper = handler as IDomainEventHandlerWrapper;

				if (wrapper != null)
					handlerType = wrapper.GetBaseHandlerType();

				throw new DomainEventHandlingException(handlerType, domainEvent, ex);
			}
		}

		private interface IDomainEventHandlerWrapper
		{
			Type GetBaseHandlerType();
		}

		/// <summary>
		/// When a base domain event handler must be passed in an enumerable, because of problems with contravariance,
		/// handlers for base types must be wrapped in an handler of the derived type. This class does this.
		/// </summary>
		/// <typeparam name="TBaseDomainEvent"></typeparam>
		/// <typeparam name="TDerivedDomainEvent"></typeparam>
		private class DomainEventHandlerWrapper<TBaseDomainEvent, TDerivedDomainEvent> : IDomainEventHandler<TDerivedDomainEvent>, IDomainEventHandlerWrapper
			where TBaseDomainEvent : IDomainEvent
			where TDerivedDomainEvent : TBaseDomainEvent
		{
			private readonly IDomainEventHandler<TBaseDomainEvent> domainEventHandler;

			public DomainEventHandlerWrapper(IDomainEventHandler<TBaseDomainEvent> domainEventHandler)
			{
				domainEventHandler.VerifyArgumentNotDefaultValue("Domain Event handler is required");

				this.domainEventHandler = domainEventHandler;
			}

			Task IDomainEventHandler<TDerivedDomainEvent>.HandleAsync(TDerivedDomainEvent domainEvent)
			{
				return this.domainEventHandler.HandleAsync(domainEvent);
			}

			Type IDomainEventHandlerWrapper.GetBaseHandlerType()
			{
				return this.domainEventHandler.GetType();
			}
		}
	}
}