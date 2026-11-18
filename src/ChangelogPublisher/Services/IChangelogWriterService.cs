using System;
using System.Collections.Generic;
using System.Text;

namespace ChangelogPublisher.Services;

internal interface IChangelogWriterService
{
    Task PublishAsync();
}
