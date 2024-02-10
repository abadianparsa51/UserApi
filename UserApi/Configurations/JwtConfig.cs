namespace UserApi.Configurations
{
    public class JwtConfig
    {
        internal double ExpiryInHours;

        public string Secret { get; set; } = "";
    }
}
