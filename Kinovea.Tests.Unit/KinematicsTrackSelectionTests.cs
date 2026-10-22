using System;
using Kinovea.ScreenManager;
using Xunit;

namespace Kinovea.Tests.Unit
{
    public class KinematicsTrackSelectionTests
    {
        [Fact]
        public void Pick_KeepsCurrentWhenStillPresent()
        {
            Guid a = Guid.NewGuid(), b = Guid.NewGuid(), c = Guid.NewGuid();

            int index = KinematicsTrackSelection.Pick(new[] { a, b, c }, new[] { true, true, true }, b);

            Assert.Equal(1, index);
        }

        [Fact]
        public void Pick_KeepsCurrentEvenWithoutData()
        {
            Guid a = Guid.NewGuid(), b = Guid.NewGuid();

            int index = KinematicsTrackSelection.Pick(new[] { a, b }, new[] { true, false }, b);

            Assert.Equal(1, index);
        }

        [Fact]
        public void Pick_PrefersFirstWithDataWhenCurrentGone()
        {
            Guid a = Guid.NewGuid(), b = Guid.NewGuid(), c = Guid.NewGuid(), x = Guid.NewGuid();

            int index = KinematicsTrackSelection.Pick(new[] { a, b, c }, new[] { false, false, true }, x);

            Assert.Equal(2, index);
        }

        [Fact]
        public void Pick_FallsBackToFirstWhenNoneHasData()
        {
            Guid a = Guid.NewGuid(), b = Guid.NewGuid();

            int index = KinematicsTrackSelection.Pick(new[] { a, b }, new[] { false, false }, Guid.Empty);

            Assert.Equal(0, index);
        }

        [Fact]
        public void Pick_EmptyListReturnsMinusOne()
        {
            int index = KinematicsTrackSelection.Pick(new Guid[0], new bool[0], Guid.NewGuid());

            Assert.Equal(-1, index);
        }

        [Fact]
        public void Pick_NewTrackDoesNotStealSelection()
        {
            Guid a = Guid.NewGuid(), b = Guid.NewGuid(), added = Guid.NewGuid();

            int index = KinematicsTrackSelection.Pick(new[] { a, b, added }, new[] { true, true, true }, a);

            Assert.Equal(0, index);
        }

        [Fact]
        public void Pick_RejectsNullAndMismatchedLists()
        {
            Guid a = Guid.NewGuid();

            Assert.Throws<ArgumentNullException>(() => KinematicsTrackSelection.Pick(null, new bool[0], a));
            Assert.Throws<ArgumentNullException>(() => KinematicsTrackSelection.Pick(new Guid[0], null, a));
            Assert.Throws<ArgumentException>(() => KinematicsTrackSelection.Pick(new[] { a }, new bool[0], a));
        }
    }
}
