using EmailOTPModule;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;

namespace Tests
{
    [TestClass]
    public class CheckOTPTests
    {
        private readonly Mock<IOTPDao> otpDaoMock;
        private readonly Mock<IOStream> ioStreamMock;
        private readonly Mock<IEmailService> emailServiceMock;
        private readonly EmailOTPModule.EmailOTPModule module;

        public CheckOTPTests()
        {
            otpDaoMock = new Mock<IOTPDao>();
            ioStreamMock = new Mock<IOStream>();
            emailServiceMock = new Mock<IEmailService>();
            module = new EmailOTPModule.EmailOTPModule(emailServiceMock.Object, otpDaoMock.Object);
        }
        [TestMethod]
        public void CheckOTP_ValidOTP_ReturnsStatusOtpOK()
        {
            // Arrange
            string expectedOTP = "123456";
            otpDaoMock.Setup(x => x.GetOTP(It.IsAny<string>())).Returns(expectedOTP);
            ioStreamMock.SetupSequence(x => x.ReadOTP())
                .Returns("123456")
                .Returns("789012");

            // Act
            string result = module.CheckOTP(ioStreamMock.Object);

            // Assert
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_OTP_OK, result);
        }

        [TestMethod]
        public void CheckOTP_ValidOTP_ReturnsStatusOtpOkAfterFiveTries()
        {
            // Arrange
            string expectedOTP = "123456";
            otpDaoMock.Setup(x => x.GetOTP(It.IsAny<string>())).Returns(expectedOTP);
            ioStreamMock.SetupSequence(x => x.ReadOTP())
                .Returns("789012")
                .Returns("427832")
                .Returns("321732")
                .Returns("abc")
                .Returns("")
                .Returns("123456");

            // Act
            string result = module.CheckOTP(ioStreamMock.Object);

            // Assert
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_OTP_OK, result);
        }

        [TestMethod]
        public void CheckOTP_InvalidOTPAfter10Tries_ReturnsStatusOtpFail()
        {
            // Arrange
            string expectedOTP = "123456";
            otpDaoMock.Setup(x => x.GetOTP(It.IsAny<string>())).Returns(expectedOTP);
            ioStreamMock.SetupSequence(x => x.ReadOTP())
                .Returns("789012")
                .Returns("427832")
                .Returns("321732")
                .Returns("abc")
                .Returns("")
                .Returns("fj")
                .Returns("127894713987214982147")
                .Returns("3u2")
                .Returns("BFEW")
                .Returns("1234567");

            // Act
            string result = module.CheckOTP(ioStreamMock.Object);

            // Assert
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_OTP_FAIL, result);
        }

        [TestMethod]
        public void CheckOTP_Timeout_ReturnsStatusOtpTimeout()
        {
            // Arrange
            string expectedOTP = "123456";
            otpDaoMock.Setup(x => x.GetOTP(It.IsAny<string>())).Returns(expectedOTP);
            ioStreamMock.Setup(x => x.ReadOTP()).Returns(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(61)); // wait for 61 seconds to simulate a timeout
                return "";
            });

            // Act
            string result = module.CheckOTP(ioStreamMock.Object);

            // Assert
            Assert.AreEqual(EmailOTPModule.EmailOTPModule.STATUS_OTP_TIMEOUT, result);
        }

    }
}