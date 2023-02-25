using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoggTree.Core;
using YoggTree.Core.Interfaces;

namespace YoggTree.Tokens.Basic
{
    public class BasicStartToken : BasicToken, IContextStarter
    {
        private string _contextStartKey = null;

        public string ContextStartKey
        { 
            get
            {
                return _contextStartKey;
            } 
        }

        public BasicStartToken(Regex token, string name, string contextKey) 
            : base(token, name)
        {
            if (string.IsNullOrEmpty(contextKey) == true) throw new ArgumentException(nameof(contextKey) + " cannot be null or empty.");
            _contextStartKey = contextKey;
        }
    }
}
