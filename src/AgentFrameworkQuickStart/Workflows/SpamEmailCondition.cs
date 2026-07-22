using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentFrameworkQuickStart.Workflows;


public static class SpamEmailCondition
{
    public static Func<object?, bool> GetCondition(bool expectedResult)
        => detectionResult => detectionResult is SpamDetectionResult result && result?.IsSpam == expectedResult;
}