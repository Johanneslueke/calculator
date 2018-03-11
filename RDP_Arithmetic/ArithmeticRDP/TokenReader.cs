using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculater_eXtreme
{
    /// <summary>
    /// Simulates similar behaviour like StringReader
    /// </summary>
    public class TokenReader
    {
        private readonly List<Token> ListOftokens = new List<Token>();
        private int                  currentIndex = -1;

        /// <summary>
        /// 
        /// </summary>
        public Token Value
        {
            get
            {
                return ListOftokens[currentIndex];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Token> Stream
        {
            get
            {
                return ListOftokens;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool TokensAvailable
        {
            get { return currentIndex < ListOftokens.Count - 1; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        public TokenReader(IEnumerable<Token> tokens)
        {
            ListOftokens = tokens.ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Token GetNext()
        {
            if (!TokensAvailable)
                throw new Exception("Cannot read anymore the end of tokens list has been reached");
            return ListOftokens[++currentIndex];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Token PeekNext()
        {
            if (!(currentIndex + 1 < ListOftokens.Count))
                throw new Exception("Cannot peek anymore the end of tokens list would have been reached");
            return ListOftokens[currentIndex + 1];
        }

        /// <summary>
        /// This we need otherwise our Tagdispatching would be for nothing!
        /// it checks which Type(Tag) we have next so we can determine somewhere
        /// else what todo next!!!
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsNextOfType(Type type)
        {
            return PeekNext().GetType() == type;
        }
    }
}
