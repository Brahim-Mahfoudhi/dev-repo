using System.Collections.Immutable;

namespace Rise.Shared.Users;
/// <summary>
/// Data Transfer Object (DTO) representing a user with minimal info.
/// </summary>
public class UserDto
{
    public record UserBase
    {
        public int Id { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public bool IsDeleted { get; init; }
        public ImmutableList<RoleDto> Roles { get; init; } = ImmutableList<RoleDto>.Empty;

        // Constructor to initialize everything
        public UserBase(int id, string firstName, string lastName, string email, 
            ImmutableList<RoleDto>? roles = null)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Roles = roles ?? ImmutableList<RoleDto>.Empty;
        }
    }

    public record UserDetails
    {
        public int Id { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public AddressDto.GetAdress Address { get; init; }
        public ImmutableList<RoleDto> Roles { get; init; } = ImmutableList<RoleDto>.Empty;
        public DateTime BirthDate { get; init; } = DateTime.Now;

        
        public UserDetails(int id, string firstName, string lastName, string email, 
            AddressDto.GetAdress address ,ImmutableList<RoleDto>? roles = null, DateTime? birthDate = null)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Address = address;
            Roles = roles ?? ImmutableList<RoleDto>.Empty;
            BirthDate = birthDate ?? DateTime.Now;
        }
    }
    
    /// <summary>
    /// DTO used for registrationform
    /// </summary>
    public record RegistrationUser
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
        public string PhoneNumber { get; init; }
        public ImmutableList<RoleDto>? Roles { get; init; } = ImmutableList<RoleDto>.Empty;
        public DateTime BirthDate { get; init; } = DateTime.Now;
        public AddressDto.CreateAddress Address { get; init; } = new AddressDto.CreateAddress();
        
        public RegistrationUser(string firstName, string lastName, string email, string phoneNumber,
            AddressDto.CreateAddress address , ImmutableList<RoleDto>? roles = null, DateTime? birthDate = null)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
            Roles = roles ?? ImmutableList<RoleDto>.Empty;
            BirthDate = birthDate ?? DateTime.Now;
        }

    }
    
    public record UpdateUser
    {
        public int Id;
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
        public DateTime? BirthDate { get; init; } = DateTime.Now;
        public AddressDto.CreateAddress Address { get; init; } = new AddressDto.CreateAddress();
        public ImmutableList<RoleDto>? Roles { get; init; } = ImmutableList<RoleDto>.Empty;
        public string PhoneNumber { get; init; }
    }

    public record CreateUserAuth0
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string? Email { get; set; } = null;
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string? FirstName { get; set; } = null;
        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string? LastName { get; set; } = null;
        /// <summary>
        /// Gets or sets the connection type to API, default is "Password-Username-Authentication"
        /// </summary>
        public string? Connection { get; set; } = "Username-Password-Authentication";
        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        public string? Password { get; set; } = null;
        
    }

    public class UserTable
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required bool Blocked { get; set; }
    }
}
