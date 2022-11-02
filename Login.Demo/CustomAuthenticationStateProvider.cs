using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace Login.Demo
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedLocalStorage storage;

        public CustomAuthenticationStateProvider(ProtectedLocalStorage storage)
        {
            this.storage = storage;
        }



        public override async Task<AuthenticationState> GetAuthenticationStateAsync()// 这个函数应当返回一个当前的用户
        {
            try
            {
                var userLocalStorage = await storage.GetAsync<User>("identity");
                var principal = CreateIdentityFromUser(userLocalStorage.Success ? userLocalStorage.Value : null);
                return new AuthenticationState(new ClaimsPrincipal(principal));
            }
            catch (Exception ex)
            {
                return new AuthenticationState(new ClaimsPrincipal(CreateIdentityFromUser(null)));
            }
        }

        public async void Login()
        {
            var user = new User
            {
                Account = "123123",
                Name = "Test",
                Age = "18",
                Password = "12321321",
                Roles = "admin,default"
            };
            if (user is not null)
            {
                await storage.SetAsync("identity", user);
                var identity = CreateIdentityFromUser(user);
                var principal = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
            }

        }
        public async void LogOut()
        {

            await storage.SetAsync("identity", new User() { Roles = ""});
            var identity = CreateIdentityFromUser(null);
            var principal = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));


        }



        private ClaimsPrincipal CreateIdentityFromUser(User? user)
        {


            if (user is not null)
            {
                var claims = new List<Claim>
                {
                    new Claim( ClaimTypes.Name,user.Name),
                    new Claim( ClaimTypes.Hash,user.Password),
                    new Claim( "age",user.Age),
                };
                claims.AddRange(user.Roles.Split(",").Select(p => new Claim(ClaimTypes.Role, p)));
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
                return claimsPrincipal;
            }
            return new ClaimsPrincipal();



        }

    }
}
