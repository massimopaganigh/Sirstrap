namespace Sirstrap.Core.Tests.Common
{
    public class SirHurtServiceTests
    {
        private readonly SirHurtService _service = new();

        [Fact]
        public void GetSirHurtPath_ReturnsNonNullString()
        {
            string path = _service.GetSirHurtPath();

            Assert.NotNull(path);
        }

        [Fact]
        public void GetSirHurtUser_ReturnsNonNullString()
        {
            string user = _service.GetSirHurtUser();

            Assert.NotNull(user);
        }
    }
}
