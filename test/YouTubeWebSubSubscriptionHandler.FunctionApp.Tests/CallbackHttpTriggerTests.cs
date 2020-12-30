using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevRelKr.YouTubeWebSubSubscriptionHandler.FunctionApp.Tests
{
    [TestClass]
    public class CallbackHttpTriggerTests
    {
        [TestMethod]
        public void Given_Null_Parameter_When_Initiated_Then_It_Should_Throw_Exception()
        {
            Action action = () => new CallbackHttpTrigger(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
