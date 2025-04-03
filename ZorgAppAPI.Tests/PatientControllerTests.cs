using Microsoft.AspNetCore.Mvc;
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
    public class PatientControllerTests
    {
        private Mock<IPatientRepository> _mockPatientRepository;
        private Mock<IAuthenticationService> _mockAuthenticationService;
        private PatientController _patientController;

        [TestInitialize]
        public void Setup()
        {
            _mockPatientRepository = new Mock<IPatientRepository>();
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _patientController = new PatientController(
                _mockPatientRepository.Object,
                _mockAuthenticationService.Object);
        }

        [TestMethod]
        public async Task GetAllPatients_ReturnsOkResult_WithListOfPatients()
        {
            // Arrange
            var patients = new List<Patient> { new Patient { ID = 1, Voornaam = "John", Achternaam = "Doe" } };
            _mockPatientRepository.Setup(repo => repo.GetPatients()).ReturnsAsync(patients);

            // Act
            var result = await _patientController.GetAllPatients();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(patients, okResult.Value);
        }

        [TestMethod]
        public async Task GetPatient_WithValidId_ReturnsOkResult_WithPatient()
        {
            // Arrange
            var patient = new Patient { ID = 1, Voornaam = "John", Achternaam = "Doe" };
            _mockPatientRepository.Setup(repo => repo.GetPatient(1)).ReturnsAsync(patient);

            // Act
            var result = await _patientController.GetPatient(1);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(patient, okResult.Value);
        }

        [TestMethod]
        public async Task GetPatient_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _mockPatientRepository.Setup(repo => repo.GetPatient(1)).ReturnsAsync((Patient)null);

            // Act
            var result = await _patientController.GetPatient(1);

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdatePatient_WithValidId_ReturnsOkResult_WithUpdatedPatient()
        {
            // Arrange
            var patientDto = new PatientDto { ID = 1, ArtsID = 2, TrajectID = 3 };
            var existingPatient = new Patient { ID = 1, Voornaam = "John", Achternaam = "Doe", ArtsID = 1, TrajectID = 1 };
            var updatedPatient = new PatientDto { ID = 1, ArtsID = 2, TrajectID = 3 };

            _mockPatientRepository.Setup(repo => repo.GetPatient(1)).ReturnsAsync(existingPatient);
            _mockPatientRepository.Setup(repo => repo.UpdatePatient(It.IsAny<PatientDto>())).ReturnsAsync(updatedPatient);

            // Act
            var result = await _patientController.UpdatePatient(patientDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(updatedPatient, okResult.Value);
        }

        [TestMethod]
        public async Task UpdatePatient_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var patientDto = new PatientDto { ID = 1, ArtsID = 2, TrajectID = 3 };
            _mockPatientRepository.Setup(repo => repo.GetPatient(1)).ReturnsAsync((Patient)null);

            // Act
            var result = await _patientController.UpdatePatient(patientDto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdatePatientAvatar_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _patientController.UpdatePatientAvatar(new PatientAvatarDto());

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task UpdatePatientAvatar_ReturnsOkResult_WithUpdatedPatientAvatar()
        {
            // Arrange
            var patientAvatarDto = new PatientAvatarDto { AvatarID = 1 };
            var updatedPatientAvatar = new PatientAvatarDto { UserId = "user1", AvatarID = 1 };

            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockPatientRepository.Setup(repo => repo.UpdatePatientAvatar(It.IsAny<PatientAvatarDto>())).ReturnsAsync(updatedPatientAvatar);

            // Act
            var result = await _patientController.UpdatePatientAvatar(patientAvatarDto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(updatedPatientAvatar, okResult.Value);
        }

        [TestMethod]
        public async Task GetPatientAvatar_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns((string)null);

            // Act
            var result = await _patientController.GetPatientAvatar();

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorizedResult);
            Assert.AreEqual(401, unauthorizedResult.StatusCode);
        }

        [TestMethod]
        public async Task GetPatientAvatar_ReturnsOkResult_WithPatientAvatar()
        {
            // Arrange
            var patientAvatar = new PatientAvatarDto { UserId = "user1", AvatarID = 1 };

            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockPatientRepository.Setup(repo => repo.GetPatientAvatar("user1")).ReturnsAsync(patientAvatar);

            // Act
            var result = await _patientController.GetPatientAvatar();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(patientAvatar, okResult.Value);
        }

        [TestMethod]
        public async Task GetPatientAvatar_PatientNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            _mockAuthenticationService.Setup(auth => auth.GetCurrentAuthenticatedUserId()).Returns("user1");
            _mockPatientRepository.Setup(repo => repo.GetPatientAvatar("user1")).ReturnsAsync((PatientAvatarDto)null);

            // Act
            var result = await _patientController.GetPatientAvatar();

            // Assert
            var notFoundResult = result as NotFoundResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
    }
}
