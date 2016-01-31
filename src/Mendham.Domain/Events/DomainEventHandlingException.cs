﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Mendham;

namespace Mendham.Domain.Events
{
	/// <summary>
	/// Exception that occurs when an event handler throws an exception when an event is rasied.
	/// The inner exception contains the details of the exception.
	/// </summary>
    public class DomainEventHandlingException : Exception
    {
		/// <summary>
		/// The domain event raised when the exception occured
		/// </summary>
		public IDomainEvent DomainEvent { get; protected set; }
		/// <summary>
		/// The type of the handler that threw an exception
		/// </summary>
		public Type DomainEventHandlerType { get; protected set; }

		private const string DEFAULT_MESSAGE = "An exception occured when handling the domain event";

		protected DomainEventHandlingException(DomainEventHandlingException firstException)
			:base(DEFAULT_MESSAGE, firstException)
		{ }

		internal DomainEventHandlingException(Type domainEventHandler, IDomainEvent domainEvent, Exception exception)
			:base(DEFAULT_MESSAGE, exception)
		{
			domainEvent.VerifyArgumentNotDefaultValue(nameof(domainEvent));
			domainEventHandler.VerifyArgumentNotDefaultValue(nameof(domainEventHandler));

			this.DomainEvent = domainEvent;
			this.DomainEventHandlerType = domainEventHandler;
		}

		public override string Message
		{
			get
			{
				return string.Format("{0} {1}. See INNER EXCEPTION for details",
					DEFAULT_MESSAGE,
					DomainEventHandlerType.FullName);
			}
		}
	}

	/// <summary>
	/// An exception that contains details when multiple domain event handlers throw an excpetion
	/// during the processing of a single domain event being raised.
	/// </summary>
	[DebuggerDisplay("Count = {Count}")]
	public class AggregateDomainEventHandlingException : DomainEventHandlingException
	{
		private readonly IReadOnlyCollection<DomainEventHandlingException> domainEventHandlingExceptions;

		internal AggregateDomainEventHandlingException(IEnumerable<DomainEventHandlingException> domainEventHandlingExceptions, DomainEventHandlingException firstException)
			:base(firstException)
		{
			domainEventHandlingExceptions
				.VerifyArgumentNotNullOrEmpty(nameof(domainEventHandlingExceptions), "The exceptions passed cannot be null or empty")
				.VerifyArgumentMeetsCriteria(a => a.Count() > 1,
                nameof(domainEventHandlingExceptions), "AggregateDomainEventHandlingException more than one exception")
                .VerifyArgumentMeetsCriteria(a => a
                    .Select(b => b.DomainEvent)
                    .Distinct()
                    .Count() == 1, nameof(domainEventHandlingExceptions), "The exceptions passed do not all have a matching domain event");

			this.DomainEvent = firstException.DomainEvent;
			this.DomainEventHandlerType = firstException.DomainEventHandlerType;

            this.domainEventHandlingExceptions = new ReadOnlyCollection<DomainEventHandlingException>(domainEventHandlingExceptions.ToList());
        }

		/// <summary>
		/// Types of the domain event handlers that threw an exception
		/// </summary>
		public IEnumerable<Type> DomainEventHandlerTypes
		{
			get
			{
				return domainEventHandlingExceptions
					.Select(a => a.DomainEventHandlerType);
			}
		}
		
		/// <summary>
		/// Exceptions thrown by domain event handlers
		/// </summary>
		public IEnumerable<DomainEventHandlingException> InnerExceptions
		{
			get
			{
				return domainEventHandlingExceptions;
			}
		}

		public override string Message
		{
			get
			{
				return string.Format("Multiple exceptions occured when handling the domain event. Exception count {0}. See See INNER EXCEPTIONS for details.",
					this.domainEventHandlingExceptions.Count());
			}
		}
	}
}