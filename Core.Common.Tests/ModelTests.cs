using Core.Common.DataModels;
using Core.Common.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Common.Tests
{
    public class ModelTests
    {
        [Fact]
        public void BaseModel_Defaults()
        {
            var before = DateTime.UtcNow;
            var product = new Product();

            Assert.False(product.IsDeleted);
            Assert.InRange(product.Created, before, DateTime.UtcNow);
            Assert.Null(product.CreatedBy);
            Assert.Null(product.LastUpdated);
            Assert.Null(product.LastUpdatedBy);
        }

        [Fact]
        public void StringIdModel_RequiresId()
        {
            Assert.False(new Tag().IsValid(out var errors));
            Assert.Contains(errors, e => e.MemberNames.Contains(nameof(Tag.Id)));
            Assert.True(new Tag { Id = "x" }.IsValid(out _));
        }

        [Theory]
        [InlineData("user@example.com", "long-enough-password", true)]
        [InlineData("not-an-email", "long-enough-password", false)]
        [InlineData("user@example.com", "short", false)]
        [InlineData("", "long-enough-password", false)]
        public void Credentials_Validation(string email, string password, bool expected)
        {
            var credentials = new Credentials { Email = email, Password = password };

            Assert.Equal(expected, credentials.IsValid(out _));
        }

        [Fact]
        public void RefreshTokenCredentials_RequiresBothFields()
        {
            Assert.False(new RefreshTokenCredentials().IsValid(out var errors));
            Assert.Equal(2, errors.Count);
            Assert.True(new RefreshTokenCredentials { UserName = "u", RefreshToken = "t" }.IsValid(out _));
        }

        [Theory]
        [InlineData(31, false)]
        [InlineData(32, true)]
        public void JWTSettings_SecretKeyMinimumLength(int length, bool expected)
        {
            var settings = new JWTSettings { SecretKey = new string('k', length), Issuer = "issuer", Audience = "audience" };

            Assert.Equal(expected, settings.IsValid(out var errors));
            if (!expected)
            {
                Assert.Contains(errors, e => e.MemberNames.Contains(nameof(JWTSettings.SecretKey)));
            }
        }

        [Fact]
        public void ApiResult_Defaults_ToEmpty()
        {
            var result = new ApiResult<Product>();

            Assert.Empty(result.Data);
            Assert.Null(result.Pagination);
        }

        [Fact]
        public void ProgressReport_DefaultMessage()
        {
            Assert.Equal("Processing", new ProgressReport().Message);
        }

        [Fact]
        public async Task BaseAuditor_LogsMessage()
        {
            var logger = new Mock<ILogger<BaseAuditor>>();
            logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var auditor = new TestAuditor(logger.Object);

            await auditor.AuditAsync("user deleted product 5", TestContext.Current.CancellationToken);

            logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => $"{state}" == "AUDIT: user deleted product 5"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        private sealed class TestAuditor(ILogger<BaseAuditor> logger) : BaseAuditor(logger);
    }
}
