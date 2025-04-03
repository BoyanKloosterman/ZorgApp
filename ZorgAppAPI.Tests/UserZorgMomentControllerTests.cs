using Microsoft.AspNetCore.Mvc;
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
    public class UserZorgMomentControllerTests
    {
        private Mock<IUserZorgMomentRepository> _mockUserZorgMomentRepository;
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private Mock<ILogger<UserZorgMomentController>> _mockLogger;
        private UserZorgMomentController _userZorgMomentController;

        [TestInitialize]
        public void Setup()
        {
            _mockUserZorgMomentRepository = new Mock<IUserZorgMomentRepository>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<UserZorgMomentController>>();
            _userZorgMomentController = new UserZorgMomentController(
                _mockUserZorgMomentRepository.Object,
                _mockAuthenticationService.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task AddUserZorgMoment_UserNotAuthenticated_ReturnsBadRequest()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _userZorgMomentController.AddUserZorgMoment(new UserZorgMomentController.ZorgmomentRequest { ZorgMomentId = 1 });

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task AddUserZorgMoment_ValidRequest_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var userId = "user1";
            var zorgMomentId = 1;
            var userZorgMoment = new UserZorgMoment { UserId = userId, ZorgMomentId = zorgMomentId };

            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns(userId);
            _mockUserZorgMomentRepository.Setup(repo => repo.AddUserZorgMomentAsync(userId, zorgMomentId)).ReturnsAsync(userZorgMoment);

            // Act
            var result = await _userZorgMomentController.AddUserZorgMoment(new UserZorgMomentController.ZorgmomentRequest { ZorgMomentId = zorgMomentId });

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(userZorgMoment, createdResult.Value);
        }

        [TestMethod]
        public async Task GetUserZorgMomentsByUserId_UserNotAuthenticated_ReturnsBadRequest()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _userZorgMomentController.GetUserZorgMomentsByUserId();

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task GetUserZorgMomentsByUserId_ValidRequest_ReturnsOkResult_WithListOfZorgMomentIds()
        {
            // Arrange
            var userId = "user1";
            var zorgMomentIds = new List<int> { 1, 2, 3 };

            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns(userId);
            _mockUserZorgMomentRepository.Setup(repo => repo.GetUserZorgMomentsByUserIdAsync(userId)).ReturnsAsync(zorgMomentIds);

            // Act
            var result = await _userZorgMomentController.GetUserZorgMomentsByUserId();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(zorgMomentIds, okResult.Value);
        }
    }
}

