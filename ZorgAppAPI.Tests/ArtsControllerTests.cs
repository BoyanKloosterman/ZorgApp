using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Models;
using ZorgAppAPI.Controllers;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Tests
{
    [TestClass]
    public class ArtsControllerTests
    {
        private Mock<IArtsRepository> _mockArtsRepository;
        private ArtsController _artsController;

        [TestInitialize]
        public void Setup()
        {
            _mockArtsRepository = new Mock<IArtsRepository>();
            _artsController = new ArtsController(_mockArtsRepository.Object);
        }

        [TestMethod]
        public async Task GetArts_ReturnsOkResult_WithListOfArts()
        {
            // Arrange
            var artsList = new List<Arts> { new Arts { ID = 1, Voornaam = "John", Achternaam = "Doe" } };
            _mockArtsRepository.Setup(repo => repo.GetArts()).ReturnsAsync(artsList);

            // Act
            var result = await _artsController.GetArts();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(artsList, okResult.Value);
        }

        [TestMethod]
        public async Task GetArts_WithValidId_ReturnsOkResult_WithArts()
        {
            // Arrange
            var arts = new Arts { ID = 1, Voornaam = "John", Achternaam = "Doe" };
            _mockArtsRepository.Setup(repo => repo.GetArts(1)).ReturnsAsync(arts);

            // Act
            var result = await _artsController.GetArts(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(arts, okResult.Value);
        }

        [TestMethod]
        public async Task GetArts_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _mockArtsRepository.Setup(repo => repo.GetArts(1)).ReturnsAsync((Arts)null);

            // Act
            var result = await _artsController.GetArts(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
    }
}
