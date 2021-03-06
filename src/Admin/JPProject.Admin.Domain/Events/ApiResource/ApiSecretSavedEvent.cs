using JPProject.Domain.Core.Events;

namespace JPProject.Admin.Domain.Events.ApiResource
{

    public class ApiSecretSavedEvent : Event
    {
        public string Type { get; }

        public ApiSecretSavedEvent(string type, string resourceName)
        {
            Type = type;
            AggregateId = resourceName;
        }
    }
}
