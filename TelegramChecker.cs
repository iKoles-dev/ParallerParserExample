namespace TGC
{
    using System;
    using System.Collections.Generic;

    using Homebrew;

    public class TelegramChecker
    {
        public bool IsError { get; set; }
        public string Url { get; private set; }
        public UrlTypes UrlType { get; private set; } 
        public int Members { get; private set; }

        private Proxy _currentProxy;
        private int triesToConnect = 3;
        public TelegramChecker(string url, int reconnects)
        {
            triesToConnect = reconnects;
            Url = url;
            LinkParser linkParser = null;
            do
            {
                ReqParametres req = new ReqParametres(url);
                _currentProxy = Proxies.GetProxy();
                req.SetProxy(_currentProxy);
                req.SetTimout(8000);
                linkParser = new LinkParser(req.Request);
            }
            while (triesToConnect-- > 0 && linkParser.IsError);

            if (!linkParser.IsError)
            {
                string title = linkParser.Data.ParsFromTo("<title>", "</title>");
                string data = linkParser.Data.ParsFromTo("<div class=\"tgme_page_extra\">", "</div>");
                SetType(title, data);
                SetMembers(data);
                Proxies.ReturnProxie(_currentProxy);
            }
            else
            {
                IsError = true;
            }
        }
        private void SetMembers(string extra)
        {
            List<string> rawData = extra.ParsRegex("([0-9 ]+)", 1);
            if (rawData.Count > 0)
            {
                if (int.TryParse(rawData[0].Replace(" ",""), out int member))
                {
                    Members = member;
                }
            }
        }

        private void SetType(string title, string extra) // 1 - chat, 2 - close chat, 3 - channel
        {
            if (title == "Telegram: Join Group Chat")
            {
                UrlType = UrlTypes.CloseChat;
                return;
            }

            string[] str_split = extra.Split(',');
            if (str_split.Length == 1)
            {
                UrlType = UrlTypes.Channel;
            }
            else
            {
                UrlType = UrlTypes.Chat;
            }
        }
    }
}