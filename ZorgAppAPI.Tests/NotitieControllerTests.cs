using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Controllers;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Tests
{
    [TestClass]
    public class NotitieControllerTests
    {
        private Mock<INotitieRepository> _mockNotitieRepository;
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private Mock<ILogger<NotitieController>> _mockLogger;
        private NotitieController _notitieController;

        [TestInitialize]
        public void Setup()
        {
            _mockNotitieRepository = new Mock<INotitieRepository>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<NotitieController>>();
            _notitieController = new NotitieController(
                _mockNotitieRepository.Object,
                _mockAuthenticationService.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetNotitieById_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notitieController.GetNotitieById(1);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetNotitieById_NotitieNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitieByIdAsync(1)).ReturnsAsync((Notitie)null);

            // Act
            var result = await _notitieController.GetNotitieById(1);

            // Assert
            var notFoundResult = result.Result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetNotitieById_NotitieFound_ReturnsOk()
        {
            // Arrange
            var notitie = new Notitie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitieByIdAsync(1)).ReturnsAsync(notitie);

            // Act
            var result = await _notitieController.GetNotitieById(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(notitie, okResult.Value);
        }

        [TestMethod]
        public async Task GetAllNotities_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notitieController.GetAllNotities();

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetAllNotities_ReturnsOk()
        {
            // Arrange
            var notities = new List<Notitie> { new Notitie { ID = 1, UserId = "user1" } };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitiesByUserIdAsync("user1")).ReturnsAsync(notities);

            // Act
            var result = await _notitieController.GetAllNotities();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(notities, okResult.Value);
        }

        [TestMethod]
        public async Task AddNotitie_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notitieController.AddNotitie(new Notitie());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task AddNotitie_ReturnsCreated()
        {
            // Arrange
            var notitie = new Notitie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.AddNotitieAsync(notitie)).Returns(Task.CompletedTask);

            // Act
            var result = await _notitieController.AddNotitie(notitie);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(notitie, createdResult.Value);
        }

        [TestMethod]
        public async Task UpdateNotitie_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notitieController.UpdateNotitie(1, new Notitie());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateNotitie_NotitieNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitieByIdAsync(1)).ReturnsAsync((Notitie)null);

            // Act
            var result = await _notitieController.UpdateNotitie(1, new Notitie { ID = 1 });

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateNotitie_ReturnsNoContent()
        {
            // Arrange
            var notitie = new Notitie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitieByIdAsync(1)).ReturnsAsync(notitie);
            _mockNotitieRepository.Setup(repo => repo.UpdateNotitieAsync(notitie)).Returns(Task.CompletedTask);

            // Act
            var result = await _notitieController.UpdateNotitie(1, notitie);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteNotitie_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _notitieController.DeleteNotitie(1);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteNotitie_NotitieNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitieByIdAsync(1)).ReturnsAsync((Notitie)null);

            // Act
            var result = await _notitieController.DeleteNotitie(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteNotitie_ReturnsNoContent()
        {
            // Arrange
            var notitie = new Notitie { ID = 1, UserId = "user1" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockNotitieRepository.Setup(repo => repo.GetNotitieByIdAsync(1)).ReturnsAsync(notitie);
            _mockNotitieRepository.Setup(repo => repo.DeleteNotitieAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _notitieController.DeleteNotitie(1);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }
    }
}
