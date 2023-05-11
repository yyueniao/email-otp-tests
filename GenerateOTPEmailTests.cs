using EmailOTPModule;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;

namespace Tests
{
    [TestClass]
    public class GenerateOTPTests
    {
        private readonly Mock<IOTPDao> otpDaoMock;
        private readonly Mock<IEmailService> emailServiceMock;
        private readonly EmailOTPModule.EmailOTPModule module;

        public GenerateOTPTests()
        {
            otpDaoMock = new Mock<IOTPDao>();
            emailServiceMock = new Mock<IEmailService>();
            module = new EmailOTPModule.EmailOTPModule(emailServiceMock.Object, otpDaoMock.Object);
        }

        [TestMethod]
        public void GenerateOTPEmail_ValidEmail_ReturnsEmailSent()
        {
            // Arrange
            string email = "tester1@dso.org.sg";

            // Act
            string result = module.GenerateOTPEmail(email);

            // Assert
            emailServiceMock.Verify(x => x.SendEmail(email, It.IsAny<string>()), Times.Once);
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_EMAIL_OK, result);
        }

        [TestMethod]
        public void GenerateOTPEmail_InvalidEmail_ReturnsEmailInvalid()
        {
            // Arrange
            string email = "invalidemail@dso.org";

            // Act
            string result = module.GenerateOTPEmail(email);

            // Assert
            emailServiceMock.Verify(x => x.SendEmail(email, It.IsAny<string>()), Times.Never);
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_EMAIL_INVALID, result);
        }

        [TestMethod]
        public void GenerateOTPEmail_EmailNotFromDSO_ReturnsEmailInvalid()
        {
            // Arrange
            string email = "tester1@proc.dso.org.sg";

            // Act
            string result = module.GenerateOTPEmail(email);

            // Assert
            emailServiceMock.Verify(x => x.SendEmail(email, It.IsAny<string>()), Times.Never);
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_EMAIL_INVALID, result);
        }

        [TestMethod]
        public void GenerateOTPEmail_EmailServiceFails_ReturnsEmailFail()
        {
            // Arrange
            string email = "tester1@dso.org.sg";
            emailServiceMock.Setup(x => x.SendEmail(email, It.IsAny<string>())).Throws<Exception>();

            // Act
            string result = module.GenerateOTPEmail(email);

            // Assert
            emailServiceMock.Verify(x => x.SendEmail(email, It.IsAny<string>()), Times.Once);
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_EMAIL_FAIL, result);
        }

        [TestMethod]
        public void GenerateOTPEmail_InvalidOTP_ReturnsEmailFail()
        {
            // Arrange
            string email = "tester1@dso.org.sg";
            otpDaoMock.Setup(x => x.CreateOTP(email, It.IsAny<string>())).Throws<Exception>();

            // Act
            string result = module.GenerateOTPEmail(email);

            // Assert
            emailServiceMock.Verify(x => x.SendEmail(email, It.IsAny<string>()), Times.Never);
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_EMAIL_FAIL, result);
        }

        [TestMethod]
        public void GenerateOTPEmail_ValidEmail_CreatesOTPCorrectly()
        {
            // Arrange
            string email = "tester1@dso.org.sg";

            // Act
            string result = module.GenerateOTPEmail(email);

            // Assert
            otpDaoMock.Verify(x => x.CreateOTP(email, It.Is<string>(s => s.Length == 6)), Times.Once);
        }
    }
}