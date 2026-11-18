using ChangelogPublisher.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogPublisher.Services;

internal interface IPublishService
{
    Task WriteChange(Change change);
}
