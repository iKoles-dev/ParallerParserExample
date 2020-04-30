namespace Homebrew
{
    public class Proxy
    {
        public string Ip { get; private set; }
        public string Port { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }

        public Proxy(string data)
        {
            string[] rawProxy = data.Split(":");
            if (rawProxy.Length >= 2)
            {
                Ip = rawProxy[0];
                Port = rawProxy[1];
            }

            if (rawProxy.Length == 4)
            {
                Login = rawProxy[2];
                Password = rawProxy[3];
            }
        }
    }
}