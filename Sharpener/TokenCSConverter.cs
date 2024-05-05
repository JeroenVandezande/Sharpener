using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharpener
{
    public static class TokenCSGenerator
    {
        private static OxygeneCodeSection _currentCodeSection = OxygeneCodeSection.None;
        private static OxygeneInterfaceSection _currentInterfaceSection = OxygeneInterfaceSection.None;
        private static string _currentClass = string.Empty;
        private static Stack<IToken> tokenStack = new Stack<IToken>();
        private static int currentScopeLevel = 0;

        /// <summary>
        /// Outputs raw CS in text form given tokens parsed from Oxygene
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string ConvertTokens(IEnumerable<IToken> tokens)
        {
            // TODO: Need to introduce the concept of Scope and class ownership to tokens for better parsing...
            // TODO: Make code section enums public so parsing can happen different depending on the file layout
            return string.Empty;
        }
    }

    internal enum OxygeneCodeSection
    {
        None,
        Interface,
        Implementation
    }

    internal enum OxygeneInterfaceSection
    {
        None,
        Uses,
        TypeDeclaration
    }
}
