#region Usings

using FluentAssertions;
using NUnit.Framework;

#endregion

namespace Stomp.Net.Test
{
    [TestFixture]
    public class BasicCalcTest
    {
        [Test]
        public void AddTest()
        {
            var target = new BasicCalc();
            var actual = target.Add( 10, 20 );

            actual.Should()
                  .Be( 30 );
        }
    }
}