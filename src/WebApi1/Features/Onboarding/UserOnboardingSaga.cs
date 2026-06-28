using Wolverine;

namespace WebApi1.Features.Onboarding;

public class UserOnboardingSaga : Saga
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsWelcomeEmailSent { get; set; }
    public DateTime StartedAt { get; set; }

    public static (UserOnboardingSaga, WelcomeEmailSent, OnboardingTimedOut) Start(UserRegistered message, ILogger<UserOnboardingSaga> logger)
    {
        logger.LogInformation("Starting onboarding saga for user {UserId}", message.UserId);

        var saga = new UserOnboardingSaga
        {
            Id = message.UserId,
            Email = message.Email,
            FirstName = message.FirstName,
            LastName = message.LastName,
            StartedAt = DateTime.UtcNow
        };

        return (
            saga,
            new WelcomeEmailSent
            {
                Id = saga.Id,
                Email = saga.Email,
                FirstName = saga.FirstName,
                LastName = saga.LastName
            },
            new OnboardingTimedOut(saga.Id));
    }

    public void Handle(WelcomeEmailSent message, ILogger<UserOnboardingSaga> logger)
    {
        logger.LogInformation("Welcome email sent for user {UserId}", message.Id);

        IsWelcomeEmailSent = true;

        MarkCompleted();
    }

    public void Handle(OnboardingTimedOut message, ILogger<UserOnboardingSaga> logger)
    {
        if (IsWelcomeEmailSent)
        {
            logger.LogInformation("Onboarding completed successfully for user {UserId}", message.Id);
            MarkCompleted();
            return;
        }

        logger.LogWarning("Onboarding timed out for user {UserId}", message.Id);

        MarkCompleted();
    }

    public static void NotFound(SendWelcomeEmail message, ILogger<UserOnboardingSaga> logger)
    {
        logger.LogWarning("Saga not found for user {UserId}. Cannot send welcome email.", message.Id);
    }

    public static void NotFound(OnboardingTimedOut message, ILogger<UserOnboardingSaga> logger)
    {
        logger.LogWarning("Saga not found for user {UserId}. Cannot handle onboarding timeout.", message.Id);
    }
}
