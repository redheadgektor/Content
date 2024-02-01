using System;

public partial class Content
{
    [Serializable]
    public class Asset
    {
        public string name;
        public string path;
        public string guid;
        public string type;
        public string base_type;

        public Asset(string name, string path, string guid, string type, string base_type)
        {
            this.name = name;
            this.path = path;
            this.guid = guid;
            this.type = type;
            this.base_type = base_type;
        }

        public bool IsScene() => type.Contains("SceneAsset", StringComparison.OrdinalIgnoreCase);
    }
}
