using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.Core;
using Wolverine;

namespace WebApi1.Features.Onboarding;

public record OnboardingTimedOut(Guid Id) : TimeoutMessage(5.Minutes())
{

}