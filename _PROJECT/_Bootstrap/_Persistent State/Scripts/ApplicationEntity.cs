using QuizCanners.Inspect;
using System;
using System.Collections.Generic;

namespace QuizCanners.IsItGame
{
    [Serializable]
    public class ApplicationEntity : IsItGameClassBase, IPEGI
    {
        public List<string> AvailableUsers = new List<string>();

        public void Inspect()
        {

        }
    }
}
