namespace WebApi1.Features.Onboarding;

public record SendWelcomeEmail(Guid Id, string Email, string FirstName, string LastName);