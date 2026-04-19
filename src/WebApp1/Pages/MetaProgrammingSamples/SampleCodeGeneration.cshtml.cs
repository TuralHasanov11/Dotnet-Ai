using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp1.Pages.MetaProgrammingSamples
{
    public class SampleCodeGeneration : PageModel
    {
        private readonly ILogger<SampleCodeGeneration> _logger;

        public SampleCodeGeneration(ILogger<SampleCodeGeneration> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            var compilerInfos = System.CodeDom.Compiler.CodeDomProvider.GetAllCompilerInfo();

            foreach (CompilerInfo compilerInfo in compilerInfos)
            {
                foreach (string language in compilerInfo.GetLanguages())
                {
                    Console.WriteLine($"Language: {language}");
                }
                Console.WriteLine();
            }
        }
    }
}