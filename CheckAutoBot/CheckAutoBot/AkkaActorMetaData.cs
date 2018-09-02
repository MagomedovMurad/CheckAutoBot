using System;
using System.Collections.Generic;
using System.Text;

namespace CheckAutoBot
{
    /// <summary>
    /// Вспомогательный класс для построения полного пути к актору с учетром его места в иерархии.
    /// </summary>
    public class AkkaActorMetaData
    {
        public AkkaActorMetaData(string name, AkkaActorMetaData parent = null)
        {
            Name = name;
            Parent = parent;
            Path = $"{parent?.Path ?? "/user"}/{Name}";
        }

        public string Name { get; }
        public string Path { get; }
        public AkkaActorMetaData Parent { get; }
    }
}
