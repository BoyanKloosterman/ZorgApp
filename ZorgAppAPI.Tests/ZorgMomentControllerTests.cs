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
    public class ZorgMomentControllerTests
    {
        private Mock<IZorgmomentRepository> _mockZorgmomentRepository;
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private Mock<ILogger<UserZorgMomentController>> _mockLogger;
        private ZorgMomentController _zorgMomentController;

        [TestInitialize]
        public void Setup()
        {
            _mockZorgmomentRepository = new Mock<IZorgmomentRepository>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<UserZorgMomentController>>();
            _zorgMomentController = new ZorgMomentController(
                _mockZorgmomentRepository.Object,
                _mockAuthenticationService.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task GetZorgmomentById_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _zorgMomentController.GetZorgmomentById(1);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetZorgmomentById_ZorgmomentNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockZorgmomentRepository.Setup(repo => repo.GetZorgmomentByIdAsync(1)).ReturnsAsync((ZorgMoment)null);

            // Act
            var result = await _zorgMomentController.GetZorgmomentById(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetZorgmomentById_ZorgmomentFound_ReturnsOk()
        {
            // Arrange
            var zorgmoment = new ZorgMoment { ID = 1, Naam = "Test Zorgmoment" };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockZorgmomentRepository.Setup(repo => repo.GetZorgmomentByIdAsync(1)).ReturnsAsync(zorgmoment);

            // Act
            var result = await _zorgMomentController.GetZorgmomentById(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(zorgmoment, okResult.Value);
        }

        [TestMethod]
        public async Task GetAllZorgmomenten_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _zorgMomentController.GetAllZorgmomenten();

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetAllZorgmomenten_ReturnsOk()
        {
            // Arrange
            var zorgmomenten = new List<int> { 1, 2, 3 };
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockZorgmomentRepository.Setup(repo => repo.GetAllZorgmomenten("user1")).ReturnsAsync(zorgmomenten);

            // Act
            var result = await _zorgMomentController.GetAllZorgmomenten();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(zorgmomenten, okResult.Value);
        }
    }
}

