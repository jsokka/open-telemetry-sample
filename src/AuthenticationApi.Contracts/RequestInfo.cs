namespace AuthenticationApi.Contracts
{
    public class RequestInfo
    {
        public required DateTimeOffset Timestamp { get; set; }

        public required string HttpMethod { get; set; }

        public required string Route { get; set; }

        public required string ClientIpAddress { get; set; }
    }
}
