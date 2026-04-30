using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp1.Pages;

public class ToBeRedirectedModel : PageModel
{
    public IActionResult OnGet()
    {
        Console.WriteLine("Redirecting to Privacy page...");
        return this.RedirectToPage<PrivacyModel>((p) => p.OnGet("Hello from ToBeRedirected!"));
    }
}