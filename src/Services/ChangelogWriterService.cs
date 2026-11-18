namespace ChangelogPublisher.Services;

internal class ChangelogWriterService : IChangelogWriterService
{
    IChangeService ChangeService { get; init; }
    IPublishService PublishService { get; init; }

    public ChangelogWriterService(
        IChangeService changeService,
        IPublishService publishService) =>
        (ChangeService, PublishService) =
            (changeService, publishService);

    public async Task PublishAsync()
    {
        var change = await ChangeService.GetChange();

        await PublishService.WriteChange(change);
    }
}
