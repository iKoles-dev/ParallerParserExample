using System;
using System.Collections.Generic;

namespace Homebrew
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public static class Proxies
    {
        private static string _proxyPath = "proxy.txt";
        private static Stack<Proxy> _proxyContainer = new Stack<Proxy>();

        public static void Load()
        {
            string rawProxies = File.ReadAllText(_proxyPath);
            foreach (var proxie in rawProxies.Split("\r\n"))
            {
                _proxyContainer.Push(new Proxy(proxie));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Proxy GetProxy()
        {
            while (_proxyContainer.Count==0)
            {
                Thread.Sleep(100);
            }
            return _proxyContainer.Pop();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void ReturnProxie(Proxy proxy)
        {
            _proxyContainer.Push(proxy);
        }

    }
}
