using Data.ViewModels;

namespace Services.Abstraction
{
    public interface IUserService
    {
        Task<UserModel> SignUp(UserModel userModel);
        Task<string> Login(LogIn userModel);
        Task<UserModel> UpdateInfoAsync(UserModel userModel);
        Task<UserModel> GetDetails();
        Task ResetPassword();
        Task DeleteAccount();
    }
}
