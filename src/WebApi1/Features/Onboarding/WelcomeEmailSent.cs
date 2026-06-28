namespace WebApi1.Features.Onboarding;

public class WelcomeEmailSent
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}