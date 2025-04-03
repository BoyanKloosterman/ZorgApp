using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Controllers;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;

namespace ZorgAppAPI.Tests
{
    [TestClass]
    public class TrajectControllerTests
    {
        private Mock<ITrajectRepository> _mockTrajectRepository;
        private TrajectController _trajectController;

        [TestInitialize]
        public void Setup()
        {
            _mockTrajectRepository = new Mock<ITrajectRepository>();
            _trajectController = new TrajectController(_mockTrajectRepository.Object);
        }

        [TestMethod]
        public async Task GetTrajectById_WithValidId_ReturnsOkResult_WithTraject()
        {
            // Arrange
            var traject = new Traject { ID = 1, Naam = "Traject 1" };
            _mockTrajectRepository.Setup(repo => repo.GetTrajectByIdAsync(1)).ReturnsAsync(traject);

            // Act
            var result = await _trajectController.GetTrajectById(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(traject, okResult.Value);
        }

        [TestMethod]
        public async Task GetTrajectById_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _mockTrajectRepository.Setup(repo => repo.GetTrajectByIdAsync(1)).ReturnsAsync((Traject)null);

            // Act
            var result = await _trajectController.GetTrajectById(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task GetAllTrajects_ReturnsOkResult_WithListOfTrajects()
        {
            // Arrange
            var trajects = new List<Traject> { new Traject { ID = 1, Naam = "Traject 1" } };
            _mockTrajectRepository.Setup(repo => repo.GetAllTrajectsAsync()).ReturnsAsync(trajects);

            // Act
            var result = await _trajectController.GetAllTrajects();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(trajects, okResult.Value);
        }

        [TestMethod]
        public async Task AddTraject_WithValidTraject_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var traject = new Traject { ID = 1, Naam = "Traject 1" };
            _mockTrajectRepository.Setup(repo => repo.AddTrajectAsync(traject)).Returns(Task.CompletedTask);

            // Act
            var result = await _trajectController.AddTraject(traject);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(traject, createdResult.Value);
        }

        [TestMethod]
        public async Task AddTraject_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _trajectController.ModelState.AddModelError("Naam", "Required");

            // Act
            var result = await _trajectController.AddTraject(new Traject());

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateTraject_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var traject = new Traject { ID = 1, Naam = "Updated Traject" };
            _mockTrajectRepository.Setup(repo => repo.UpdateTrajectAsync(traject)).Returns(Task.CompletedTask);

            // Act
            var result = await _trajectController.UpdateTraject(1, traject);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateTraject_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var traject = new Traject { ID = 1, Naam = "Updated Traject" };

            // Act
            var result = await _trajectController.UpdateTraject(2, traject);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdateTraject_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var traject = new Traject { ID = 1, Naam = "Updated Traject" };
            _trajectController.ModelState.AddModelError("Naam", "Required");

            // Act
            var result = await _trajectController.UpdateTraject(1, traject);

            // Assert
            var badRequestResult = result as BadRequestResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public async Task DeleteTraject_WithValidId_ReturnsNoContent()
        {
            // Arrange
            _mockTrajectRepository.Setup(repo => repo.DeleteTrajectAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _trajectController.DeleteTraject(1);

            // Assert
            var noContentResult = result as NoContentResult;
            Assert.IsNotNull(noContentResult);
            Assert.AreEqual(204, noContentResult.StatusCode);
        }
    }
}

