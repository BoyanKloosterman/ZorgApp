using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Controllers;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Tests
{
    [TestClass]
    public class NotificatieControllerTests
    {
        private Mock<INotificatieRepository> _mockNotificatieRepository;
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private Mock<ILogger<NotificatieController>> _mockLogger;
        private NotificatieController _notificatieController;

        [TestInitialize]
        public void Setup()
        {
            _mockNotificatieRepository = new Mock<INotificatieRepository>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<NotificatieController>>();
            _notificatieController = new NotificatieController(
                _mockNotificatieRepository.Object,
                _mockAuthenticationService.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetNotificatieById_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notificatieController.GetNotificatieById(1);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetNotificatieById_NotificatieNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatieByIdAsync(1)).ReturnsAsync((Notificatie)null);

            // Act
            var result = await _notificatieController.GetNotificatieById(1);

            // Assert
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetNotificatieById_NotificatieFound_ReturnsOk()
        {
            // Arrange
            var notificatie = new Notificatie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatieByIdAsync(1)).ReturnsAsync(notificatie);

            // Act
            var result = await _notificatieController.GetNotificatieById(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(notificatie, okResult.Value);
        }

        [TestMethod]
        public async Task GetAllNotificaties_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notificatieController.GetAllNotificaties();

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetAllNotificaties_ReturnsOk()
        {
            // Arrange
            var notificaties = new List<Notificatie> { new Notificatie { ID = 1, UserId = "user1" } };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatiesByUserIdAsync("user1")).ReturnsAsync(notificaties);

            // Act
            var result = await _notificatieController.GetAllNotificaties();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(notificaties, okResult.Value);
        }

        [TestMethod]
        public async Task AddNotificatie_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notificatieController.AddNotificatie(new Notificatie());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task AddNotificatie_ReturnsCreated()
        {
            // Arrange
            var notificatie = new Notificatie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.AddNotificatieAsync(notificatie)).Returns(Task.CompletedTask);

            // Act
            var result = await _notificatieController.AddNotificatie(notificatie);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(notificatie, createdResult.Value);
        }

        [TestMethod]
        public async Task UpdateNotificatie_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notificatieController.UpdateNotificatie(1, new Notificatie());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateNotificatie_NotificatieNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatieByIdAsync(1)).ReturnsAsync((Notificatie)null);

            // Act
            var result = await _notificatieController.UpdateNotificatie(1, new Notificatie { ID = 1 });

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateNotificatie_ReturnsNoContent()
        {
            // Arrange
            var notificatie = new Notificatie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatieByIdAsync(1)).ReturnsAsync(notificatie);
            _mockNotificatieRepository.Setup(repo => repo.UpdateNotificatieAsync(notificatie)).Returns(Task.CompletedTask);

            // Act
            var result = await _notificatieController.UpdateNotificatie(1, notificatie);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteNotificatie_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notificatieController.DeleteNotificatie(1);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteNotificatie_NotificatieNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatieByIdAsync(1)).ReturnsAsync((Notificatie)null);

            // Act
            var result = await _notificatieController.DeleteNotificatie(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteNotificatie_ReturnsNoContent()
        {
            // Arrange
            var notificatie = new Notificatie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.GetNotificatieByIdAsync(1)).ReturnsAsync(notificatie);
            _mockNotificatieRepository.Setup(repo => repo.DeleteNotificatieAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _notificatieController.DeleteNotificatie(1);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }

        [TestMethod]
        public async Task SendTestWebSocket_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notificatieController.SendTestWebSocket();

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task SendTestWebSocket_ReturnsOk()
        {
            // Arrange
            var notificatie = new Notificatie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotificatieRepository.Setup(repo => repo.AddNotificatieAsync(It.IsAny<Notificatie>())).Returns(Task.CompletedTask);
            var mockNotificatieSender = new Mock<INotificatieSender>();
            mockNotificatieSender.Setup(sender => sender.SendNotificationAsync(It.IsAny<Notificatie>())).Returns(Task.CompletedTask);
            _notificatieController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _notificatieController.ControllerContext.HttpContext.RequestServices = new ServiceCollection()
                .AddSingleton(mockNotificatieSender.Object)
                .BuildServiceProvider();

            // Act
            var result = await _notificatieController.SendTestWebSocket("Test message");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }
    }
}
