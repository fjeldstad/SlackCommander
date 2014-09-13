using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magnum.Extensions;
using MassTransit;
using MassTransit.TestFramework;
using MassTransit.TestFramework.Fixtures;
using NUnit.Framework;

namespace SlackCommander.Web.Tests
{
    public class TestConsumerOf<TMessage> : 
        Consumes<TMessage>.All 
        where TMessage : class
    {
        static readonly List<TMessage> _allMessages = new List<TMessage>();
		static readonly Semaphore _allReceived = new Semaphore(0, 100);
		static int _allReceivedCount;
		readonly Action<TMessage> _consumerAction;

		readonly List<TMessage> _messages = new List<TMessage>();
		readonly Semaphore _received = new Semaphore(0, 100);

		int _receivedMessageCount;

		public TestConsumerOf()
		{
			_consumerAction = x => { };
		}

        public TestConsumerOf(Action<TMessage> consumerAction)
		{
			_consumerAction = consumerAction;
		}

		public static int AllReceivedCount
		{
			get { return _allReceivedCount; }
		}

		public int ReceivedMessageCount
		{
			get { return _receivedMessageCount; }
			protected set { _receivedMessageCount = value; }
		}

		public virtual void Consume(TMessage message)
		{
			Interlocked.Increment(ref _receivedMessageCount);
			Interlocked.Increment(ref _allReceivedCount);

			_consumerAction(message);

			_messages.Add(message);
			_received.Release();

			_allMessages.Add(message);
			_allReceived.Release();
		}

		public void MessageHandler(TMessage message)
		{
			_messages.Add(message);
			_received.Release();
		}

        public void ShouldHaveReceivedAny(TimeSpan timeout)
        {
            Assert.IsTrue(ReceivedMessage(m => m != null, timeout));
        }

        public void ShouldHaveReceived(Func<TMessage, bool> predicate)
		{
			ShouldHaveReceived(predicate, 0.Seconds());
		}

        public void ShouldHaveReceived(Func<TMessage, bool> predicate, TimeSpan timeout)
		{
			Assert.IsTrue(ReceivedMessage(predicate, timeout), "Message should have been received");
		}

        public void ShouldNotHaveReceivedMessage(Func<TMessage, bool> predicate)
		{
			Assert.That(ReceivedMessage(predicate, 0.Seconds()), Is.False, "Message should not have been received");
		}

        public void ShouldNotHaveReceivedMessage(Func<TMessage, bool> predicate, TimeSpan timeout)
		{
			Assert.That(ReceivedMessage(predicate, timeout), Is.False, "Message should not have been received");
		}

        bool ReceivedMessage(Func<TMessage, bool> predicate, TimeSpan timeout)
		{
			while (_messages.Any(predicate) == false)
			{
				if (_received.WaitOne(timeout, true) == false)
					return false;
			}

			return true;
		}

        public static void AnyShouldHaveReceivedMessage(Func<TMessage, bool> predicate, TimeSpan timeout)
		{
			Assert.That(AnyReceivedMessage(predicate, timeout), Is.True, "Message should have been received");
		}

        public static void OnlyOneShouldHaveReceivedMessage(Func<TMessage, bool> predicate, TimeSpan timeout)
		{
			Assert.That(AnyReceivedMessage(predicate, timeout), Is.True, "Message should have been received");
		}

		static bool AnyReceivedMessage(Func<TMessage, bool> predicate, TimeSpan timeout)
		{
			while (_allMessages.Any(predicate) == false)
			{
				if (_allReceived.WaitOne(timeout, true) == false)
					return false;
			}

			return true;
		}
    }
}
