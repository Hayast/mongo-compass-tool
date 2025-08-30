namespace mongo
{
    // 连接信息结构
    public class MongoConnInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuthMechanism { get; set; }
        public string AuthSource { get; set; }
        public override string ToString() { return string.Format("{0} ({1}:{2}/{3})", Name, Host, Port, Database); }
    }
} 