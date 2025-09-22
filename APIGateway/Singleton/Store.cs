namespace APIGateway.Singleton
{
    public class Store
    {
        public Store() { }
        private static readonly Store _instance = new Store();

        public static Store Instance => _instance;

        public bool IsDevelopment { get; set; }

        public List<string> noAuthroutes { get; set; }
    }
}
