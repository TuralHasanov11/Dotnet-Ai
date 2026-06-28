using JasperFx.Core;
using Wolverine;

namespace WebApi1.Features.Onboarding;

public record OnboardingTimedOut(Guid Id) : TimeoutMessage(5.Minutes())
{

}