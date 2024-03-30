using System;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

   [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        Console.WriteLine("Registering user");
        Console.WriteLine(model.Name);
        Console.WriteLine(model.Surname);
        Console.WriteLine(model.Specjalization);
        Console.WriteLine(model.Username);
        Console.WriteLine(model.Password);
        Console.WriteLine(model.Activated);

            var newUser = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                Specjalization = model.Specjalization,
                UserName = model.Username,
                Password = model.Password,
                Activated = model.Activated
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                // Add the user to the Patient/Doctor role depending on activated
                var roleResult = new IdentityResult();
                if(newUser.Activated == true){
                    roleResult = await _userManager.AddToRoleAsync(newUser, "Doctor");
                }else{
                    roleResult = await _userManager.AddToRoleAsync(newUser, "Patient");
                }
                if (roleResult.Succeeded)
                {
                    // await _signInManager.SignInAsync(newUser, isPersistent: false);
                    return Json(new { success = true });
                }
                else
                {
                    var errors = roleResult.Errors.Select(e => e.Description);
                    return Json(new { success = false, error = string.Join(", ", errors) });
                }
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description);
                return Json(new { success = false, error = string.Join(", ", errors) });
            }
            
        

        // return Json(new { success = false, error = "Invalid data" });
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Json(new { success = false, error = "Invalid login attempt." });
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
