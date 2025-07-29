using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.DTOs;
using AutoMapper;

namespace WorkIntakeSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "SystemAdministrator,DepartmentManager")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpGet("department/{departmentId}")]
        [Authorize(Roles = "SystemAdministrator,DepartmentManager")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var users = await _userRepository.GetByDepartmentAsync(departmentId);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("business-vertical/{businessVerticalId}")]
        [Authorize(Roles = "SystemAdministrator,DepartmentManager")]
        public async Task<IActionResult> GetByBusinessVertical(int businessVerticalId)
        {
            var users = await _userRepository.GetByBusinessVerticalAsync(businessVerticalId);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SystemAdministrator")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            // Update user properties
            user.Name = request.Name;
            user.DepartmentId = request.DepartmentId;
            user.BusinessVerticalId = request.BusinessVerticalId;
            user.Role = Enum.Parse<WorkIntakeSystem.Core.Enums.UserRole>(request.Role);
            user.Capacity = request.Capacity;
            user.SkillSet = request.SkillSet;
            user.ModifiedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SystemAdministrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userRepository.DeleteAsync(id);
            return NoContent();
        }
    }

    public class UpdateUserRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int BusinessVerticalId { get; set; }
        public string Role { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string SkillSet { get; set; } = string.Empty;
    }
} 