namespace CinemaTAPS.API.Services;

using CinemaTAPS.API.Models;

public interface IUserService
{
    Task<(bool Success, string ErrorMessage)> EditUserProfileAsync(int userId, string firstName, string lastName, string phoneNumber, Guid concurrencyToken, int modifierUserId, bool isAdmin);
}
