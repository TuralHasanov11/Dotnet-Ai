namespace WebApi1.Features.Onboarding;

public class SendWelcomeEmailHandler
{
    public static async Task<WelcomeEmailSent> Handle(SendWelcomeEmail command, ILogger<SendWelcomeEmailHandler> logger)
    {
        logger.LogInformation("Sending welcome email to user {Email}", command.Email);

        // Simulate sending the email
        await Task.Delay(1000); // Simulate some delay for sending the email

        logger.LogInformation("Welcome email sent to user {Email}", command.Email);

        return new WelcomeEmailSent
        {
            Id = command.Id,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName
        };
    }
}