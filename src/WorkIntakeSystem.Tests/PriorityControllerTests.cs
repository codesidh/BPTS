using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WorkIntakeSystem.API.Controllers;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Services;
using WorkIntakeSystem.Infrastructure.Data;
using Xunit;

namespace WorkIntakeSystem.Tests
{
    public class PriorityControllerTests
    {
        private readonly Mock<IPriorityRepository> _mockPriorityRepository;
        private readonly Mock<IWorkRequestRepository> _mockWorkRequestRepository;
        private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
        private readonly Mock<IPriorityCalculationService> _mockPriorityCalculationService;
        private readonly Mock<ILogger<PriorityController>> _mockLogger;
        private readonly PriorityController _controller;

        public PriorityControllerTests()
        {
            _mockPriorityRepository = new Mock<IPriorityRepository>();
            _mockWorkRequestRepository = new Mock<IWorkRequestRepository>();
            _mockDepartmentRepository = new Mock<IDepartmentRepository>();
            _mockPriorityCalculationService = new Mock<IPriorityCalculationService>();
            _mockLogger = new Mock<ILogger<PriorityController>>();

            _controller = new PriorityController(
                _mockPriorityRepository.Object,
                _mockWorkRequestRepository.Object,
                _mockDepartmentRepository.Object,
                _mockPriorityCalculationService.Object,
                null!, // IMapper
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SubmitVote_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new PriorityVoteRequest
            {
                WorkRequestId = 1,
                Vote = PriorityVote.High,
                BusinessValueScore = 0.8m,
                StrategicAlignment = 0.7m,
                Comments = "High priority for business"
            };

            var workRequest = new WorkRequest
            {
                Id = 1,
                Title = "Test Work Request",
                Status = WorkStatus.Submitted,
                Priority = 0.5m
            };

            var user = new User
            {
                Id = 1,
                Name = "Test User",
                DepartmentId = 1
            };

            _mockWorkRequestRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(workRequest);

            _mockPriorityRepository.Setup(x => x.GetByWorkRequestAndDepartmentAsync(1, 1))
                .ReturnsAsync((Priority?)null);

            _mockPriorityRepository.Setup(x => x.CreateAsync(It.IsAny<Priority>()))
                .ReturnsAsync(new Priority());

            _mockPriorityCalculationService.Setup(x => x.UpdatePriorityAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SubmitVote(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PriorityVoteResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Vote submitted successfully", response.Message);
        }

        [Fact]
        public async Task SubmitVote_WorkRequestNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new PriorityVoteRequest
            {
                WorkRequestId = 999,
                Vote = PriorityVote.High
            };

            _mockWorkRequestRepository.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((WorkRequest?)null);

            // Act
            var result = await _controller.SubmitVote(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task SubmitVote_DepartmentAlreadyVoted_ReturnsBadRequest()
        {
            // Arrange
            var request = new PriorityVoteRequest
            {
                WorkRequestId = 1,
                Vote = PriorityVote.High
            };

            var workRequest = new WorkRequest { Id = 1, Title = "Test" };
            var existingVote = new Priority { Id = 1, WorkRequestId = 1, DepartmentId = 1 };

            _mockWorkRequestRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(workRequest);

            _mockPriorityRepository.Setup(x => x.GetByWorkRequestAndDepartmentAsync(1, 1))
                .ReturnsAsync(existingVote);

            // Act
            var result = await _controller.SubmitVote(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetVotingStatus_ValidRequest_ReturnsVotingStatus()
        {
            // Arrange
            var workRequestId = 1;
            var workRequest = new WorkRequest
            {
                Id = workRequestId,
                Title = "Test Work Request",
                Priority = 0.75m,
                PriorityLevel = PriorityLevel.High
            };

            var votes = new List<Priority>
            {
                new Priority { DepartmentId = 1, Vote = PriorityVote.High },
                new Priority { DepartmentId = 2, Vote = PriorityVote.Medium }
            };

            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "Dept 1" },
                new Department { Id = 2, Name = "Dept 2" },
                new Department { Id = 3, Name = "Dept 3" }
            };

            _mockWorkRequestRepository.Setup(x => x.GetByIdAsync(workRequestId))
                .ReturnsAsync(workRequest);

            _mockPriorityRepository.Setup(x => x.GetByWorkRequestIdAsync(workRequestId))
                .ReturnsAsync(votes);

            _mockDepartmentRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(departments);

            // Act
            var result = await _controller.GetVotingStatus(workRequestId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var votingStatus = Assert.IsType<PriorityVotingStatus>(okResult.Value);
            
            Assert.Equal(workRequestId, votingStatus.WorkRequestId);
            Assert.Equal(3, votingStatus.TotalDepartments);
            Assert.Equal(2, votingStatus.VotedDepartments);
            Assert.Equal(1, votingStatus.PendingDepartments);
            Assert.Equal(0.75m, votingStatus.CurrentPriorityScore);
            Assert.Equal(PriorityLevel.High, votingStatus.CurrentPriorityLevel);
        }

        [Fact]
        public async Task GetPendingVotes_ValidDepartment_ReturnsPendingVotes()
        {
            // Arrange
            var departmentId = 1;
            var pendingVotes = new List<PendingVoteInfo>
            {
                new PendingVoteInfo { WorkRequestId = 1 },
                new PendingVoteInfo { WorkRequestId = 2 }
            };

            var workRequests = new List<WorkRequest>
            {
                new WorkRequest { Id = 1, Title = "Work Request 1", Category = WorkCategory.WorkRequest, PriorityLevel = PriorityLevel.High, CreatedDate = DateTime.UtcNow.AddDays(-5), Submitter = new User { Name = "User 1" } },
                new WorkRequest { Id = 2, Title = "Work Request 2", Category = WorkCategory.Project, PriorityLevel = PriorityLevel.Medium, CreatedDate = DateTime.UtcNow.AddDays(-3), Submitter = new User { Name = "User 2" } }
            };

            _mockPriorityRepository.Setup(x => x.GetPendingVotesForDepartmentAsync(departmentId))
                .ReturnsAsync(pendingVotes);

            _mockWorkRequestRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(workRequests);

            // Act
            var result = await _controller.GetPendingVotes(departmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pendingVotesResult = Assert.IsType<IEnumerable<PendingVote>>(okResult.Value);
            var pendingVotesList = pendingVotesResult.ToList();
            
            Assert.Equal(2, pendingVotesList.Count);
            Assert.Equal("Work Request 1", pendingVotesList[0].Title);
            Assert.Equal("Work Request 2", pendingVotesList[1].Title);
        }

        [Fact]
        public async Task GetDepartmentVotingPattern_ValidDepartment_ReturnsVotingPattern()
        {
            // Arrange
            var departmentId = 1;
            var department = new Department { Id = departmentId, Name = "Test Department" };

            var votes = new List<Priority>
            {
                new Priority { Vote = PriorityVote.High, BusinessValueScore = 0.8m, StrategicAlignment = 0.7m, CreatedDate = DateTime.UtcNow.AddDays(-1) },
                new Priority { Vote = PriorityVote.Medium, BusinessValueScore = 0.6m, StrategicAlignment = 0.5m, CreatedDate = DateTime.UtcNow.AddDays(-2) },
                new Priority { Vote = PriorityVote.High, BusinessValueScore = 0.9m, StrategicAlignment = 0.8m, CreatedDate = DateTime.UtcNow.AddDays(-3) }
            };

            _mockDepartmentRepository.Setup(x => x.GetByIdAsync(departmentId))
                .ReturnsAsync(department);

            _mockPriorityRepository.Setup(x => x.GetByDepartmentIdAsync(departmentId))
                .ReturnsAsync(votes);

            // Act
            var result = await _controller.GetDepartmentVotingPattern(departmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pattern = Assert.IsType<DepartmentVotingPattern>(okResult.Value);
            
            Assert.Equal(departmentId, pattern.DepartmentId);
            Assert.Equal("Test Department", pattern.DepartmentName);
            Assert.Equal(3, pattern.TotalVotes);
            Assert.Equal(2, pattern.VoteDistribution[PriorityVote.High]);
            Assert.Equal(1, pattern.VoteDistribution[PriorityVote.Medium]);
            Assert.Equal(0.77m, pattern.AverageBusinessValueScore, 2);
            Assert.Equal(0.67m, pattern.AverageStrategicAlignment, 2);
        }

        [Fact]
        public async Task UpdateVote_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var workRequestId = 1;
            var request = new PriorityVoteRequest
            {
                WorkRequestId = workRequestId,
                Vote = PriorityVote.Low,
                BusinessValueScore = 0.3m,
                StrategicAlignment = 0.4m,
                Comments = "Updated vote"
            };

            var existingVote = new Priority
            {
                Id = 1,
                WorkRequestId = workRequestId,
                DepartmentId = 1,
                Vote = PriorityVote.High
            };

            var workRequest = new WorkRequest
            {
                Id = workRequestId,
                Title = "Test Work Request",
                Priority = 0.6m
            };

            _mockPriorityRepository.Setup(x => x.GetByWorkRequestAndDepartmentAsync(workRequestId, 1))
                .ReturnsAsync(existingVote);

            _mockPriorityRepository.Setup(x => x.UpdateAsync(It.IsAny<Priority>()))
                .Returns(Task.CompletedTask);

            _mockPriorityCalculationService.Setup(x => x.UpdatePriorityAsync(workRequestId))
                .Returns(Task.CompletedTask);

            _mockWorkRequestRepository.Setup(x => x.GetByIdAsync(workRequestId))
                .ReturnsAsync(workRequest);

            // Act
            var result = await _controller.UpdateVote(workRequestId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PriorityVoteResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Vote updated successfully", response.Message);
        }

        [Fact]
        public async Task UpdateVote_VoteNotFound_ReturnsNotFound()
        {
            // Arrange
            var workRequestId = 1;
            var request = new PriorityVoteRequest
            {
                WorkRequestId = workRequestId,
                Vote = PriorityVote.Low
            };

            _mockPriorityRepository.Setup(x => x.GetByWorkRequestAndDepartmentAsync(workRequestId, 1))
                .ReturnsAsync((Priority?)null);

            // Act
            var result = await _controller.UpdateVote(workRequestId, request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
} 