﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapestry.TokenDefinitions
{
    public interface ITokenDefinition
    {
        TokenType Type { get; }

        char StartCharacter { get; }

        Token Create(AbstractLexer lexer, SStreamReader reader);
    }
}