using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class PersistentStorage : MonoBehaviour
    {
        private string _savePath;

        private void Awake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "saveFile");
        }
        
        public void Save (PersistableObject o, int version) {
            using (
                var writer = new BinaryWriter(File.Open(_savePath, FileMode.Create))
            ) {
                writer.Write(-version);
                o.Save(new GameDataWriter(writer));
            }
        }

        public void Load (PersistableObject o) {
            var data = File.ReadAllBytes(_savePath);
            MemoryStream ms = new MemoryStream(data);
            var reader = new BinaryReader(ms);
            o.Load(new GameDataReader(reader, -reader.ReadInt32()));
        }
    }
}