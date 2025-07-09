using System.IO;
using Content.Scripts.Game.IO.Structures;
using Content.Scripts.Game.Voxels;

namespace Content.Scripts.Game.IO
{
    public partial class VoxelVolumeIO
    {
        private void SaveWithCompression(SavedVolume data, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                // Записываем основные параметры
                writer.Write(data.boundsSize.x);
                writer.Write(data.boundsSize.y);
                writer.Write(data.boundsSize.z);

                writer.Write(data.chunkSize.x);
                writer.Write(data.chunkSize.y);
                writer.Write(data.chunkSize.z);

                writer.Write(data.voxelSize);

                // Записываем количество чанков
                writer.Write(data.chunks.Length);

                foreach (var chunk in data.chunks)
                {
                    // Позиция чанка
                    writer.Write(chunk.chunkPosition.x);
                    writer.Write(chunk.chunkPosition.y);
                    writer.Write(chunk.chunkPosition.z);

                    // Количество блоков данных
                    writer.Write(chunk.blocksData.Length);

                    foreach (var blockData in chunk.blocksData)
                    {
                        writer.Write(blockData.id);
                        writer.Write(blockData.lineLength);

                        // Сериализуем Block структуру (предполагая, что Block имеет простые поля)
                        WriteBlock(writer, blockData.block);
                    }
                }

                writer.Write(data.position.x);
                writer.Write(data.position.y);
                writer.Write(data.position.z);

                writer.Write(data.rotation.x);
                writer.Write(data.rotation.y);
                writer.Write(data.rotation.z);
            }
        }

        public SavedVolume LoadWithCompression(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            using (var reader = new BinaryReader(fileStream))
            {
                // Читаем основные параметры
                var boundsSize = new Vector3IntData(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
                var chunkSize = new Vector3IntData(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());
                var voxelSize = reader.ReadSingle();


                var data = new SavedVolume(boundsSize.Convert(), chunkSize.Convert(), voxelSize);


                // Читаем чанки
                int chunksCount = reader.ReadInt32();
                var chunks = new ChunkData[chunksCount];

                for (int i = 0; i < chunksCount; i++)
                {
                    // Позиция чанка
                    var chunkPos = new Vector3IntData(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16());

                    // Блоки данных
                    int blocksCount = reader.ReadInt32();
                    var blocksData = new BlockData[blocksCount];

                    for (int j = 0; j < blocksCount; j++)
                    {
                        var id = reader.ReadUInt16();
                        var lineLength = reader.ReadUInt16();
                        var block = ReadBlock(reader);

                        blocksData[j] = new BlockData(id, block, lineLength);
                    }

                    chunks[i] = new ChunkData { chunkPosition = chunkPos, blocksData = blocksData };
                }

                if (reader.PeekChar() != -1)
                {
                    var pos = new Vector3Data(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    var rot = new Vector3Data(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    data.SetPos(pos, rot);
                }

                data.chunks = chunks;
                return data;
            }
        }

        private void WriteBlock(BinaryWriter writer, Block block)
        {
            writer.Write(block.type);
            writer.Write(block.flags);
            writer.Write(block.materialId);
            // Добавьте другие поля Block, если они есть
            // writer.Write(block.health);
            // writer.Write(block.color.r);
            // и т.д.
        }

        private Block ReadBlock(BinaryReader reader)
        {
            var block = new Block();
            block.type = reader.ReadByte(); // или другой тип данных
            block.flags = reader.ReadByte(); // или другой тип данных
            block.materialId = reader.ReadByte(); // или другой тип данных
            // Читайте другие поля Block, если они есть
            // block.health = reader.ReadInt32();
            // block.color.r = reader.ReadSingle();
            // и т.д.
            return block;
        }
    }
}