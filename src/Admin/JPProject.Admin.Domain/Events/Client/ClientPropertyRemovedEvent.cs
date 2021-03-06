using JPProject.Domain.Core.Events;

namespace JPProject.Admin.Domain.Events.Client
{
    public class ClientPropertyRemovedEvent : Event
    {
        public string Key { get; }
        public string Value { get; }

        public ClientPropertyRemovedEvent(string key, string value, string clientId)
        {
            Key = key;
            Value = value;
            AggregateId = clientId;
        }
    }

}