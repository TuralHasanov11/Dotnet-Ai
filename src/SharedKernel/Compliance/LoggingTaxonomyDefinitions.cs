using Microsoft.Extensions.Compliance.Classification;

namespace SharedKernel.Compliance;

/// <summary>
/// Provides reusable <see cref="DataClassification"/> definitions for logging and compliance.
/// </summary>
/// <summary>
/// Provides taxonomy definitions for data classification used in logging and compliance.
/// </summary>
public static class LoggingTaxonomyDefinitions
{
    /// <summary>
    /// Data classification for End User Identifiable Information (EUII).
    /// </summary>
    public static DataClassification EUIIDataClassification => new("EUIIDataTaxonomy", "EUII");


    /// <summary>
    /// Data classification for End User Pseudonymous Data (EUP).
    /// </summary>
    public static DataClassification EUPDataClassification => new("EUPDataTaxonomy", "EUP");

    /// <summary>
    /// Data classification for Customer Data as defined by Microsoft Purview.
    /// Customer Data includes user content such as emails, files, Teams chat, and Copilot interactions.
    /// </summary>
    public static DataClassification CustomerDataClassification => new("CustomerDataTaxonomy", "CustomerData");

    /// <summary>
    /// Data classification for Administrator Data.
    /// Administrator Data includes tenant admin's email, UPN, IP address, username, and display name.
    /// </summary>
    public static DataClassification AdministratorDataClassification => new("AdministratorDataTaxonomy", "AdministratorData");

    /// <summary>
    /// Data classification for Feedback Data.
    /// Feedback Data is feedback about compliance solutions submitted by tenant administrators.
    /// </summary>
    public static DataClassification FeedbackDataClassification => new("FeedbackDataTaxonomy", "FeedbackData");
}