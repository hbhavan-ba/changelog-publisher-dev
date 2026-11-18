using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogPublisher.Data;

/// <summary>
/// Data for the change to be published
/// </summary>
/// <param name="Timestamp">Time change is made</param>
/// <param name="Author">Author of change</param>
/// <param name="Description">Overall description of change</param>
/// <param name="Details">Additional details of chnage</param>
internal record Change(
    DateTime Timestamp,
    string Author,
    string Title,
    string Description,
    IEnumerable<string> Details);

public static class ChangeImpl
{
    extension(Change change)
    {
        internal string FormatMessage()
        {
            var dateString = change.Timestamp.ToString("MMMM-dd-yyyy");

            return $"""
                Date: {dateString}
                Authors: {change.Author}
                Description: {change.Description}
                Details:
                {string.Join("\t-" + Environment.NewLine, change.Details)}
                """;
        }
    }
}
