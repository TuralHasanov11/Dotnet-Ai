using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApp1.Pages;

public class ToBeRedirectedModel : PageModel
{
    public IActionResult OnGet()
    {
        Console.WriteLine("Redirecting to Privacy page...");
        return this.RedirectToPage<PrivacyModel>((p) => p.OnGet("Hello from ToBeRedirected!"));
    }
}